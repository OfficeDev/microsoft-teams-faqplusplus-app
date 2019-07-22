// <copyright file="ConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// ConfigurationProvider which will help in fetching and storing information in storage table.
    /// </summary>
    public class ConfigurationProvider : IConfigurationProvider
    {
        private const string QnAMakerRequestUrl = "https://westus.api.cognitive.microsoft.com/qnamaker/v4.0";
        private const string MethodKB = "knowledgebases";

        private const string TeamPartitionKey = "TeamInfo";
        private const string TeamRowKey = "MSTeamId";
        private const string KnowledgeBasePartitionKey = "KnowledgeBaseInfo";
        private const string KnowledgeBaseRowKey = "KnowledgeBaseId";
        private const string WelcomeMessagePartitionKey = "WelcomeInfo";
        private const string WelcomeMessageRowKey = "WelcomeMessage";
        private const string StaticTabPartitionKey = "StaticTabInfo";
        private const string StaticTabRowKey = "StaticTabText";
        private const string TicketPartitionKey = "TicketInfo";

        private const string TeamIdEscapedStartString = "19%3a";
        private const string TeamIdEscapedEndString = "%40thread.skype";
        private const string TeamIdUnescapedStartString = "19:";
        private const string TeamIdUnescapedEndString = "@thread.skype";

        private readonly Lazy<Task> initializeTask;
        private CloudTable configurationCloudTable;
        private CloudTable ticketCloudTable;
        private HttpClient httpClient;
        private string qnaMakerSubscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public ConfigurationProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString));
        }

        /// <inheritdoc/>
        public async Task<bool> SaveOrUpdateEntityAsync(string updatedData, string entityType, TicketEntity ticketEntity = null)
        {
            try
            {
                dynamic entity = null;
                switch (entityType)
                {
                    case Constants.TeamEntityType:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = TeamPartitionKey,
                            RowKey = TeamRowKey,
                            Data = updatedData
                        };
                        break;

                    case Constants.KnowledgeBaseEntityType:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = KnowledgeBasePartitionKey,
                            RowKey = KnowledgeBaseRowKey,
                            Data = updatedData
                        };
                        break;

                    case Constants.WelcomeMessageEntityType:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = WelcomeMessagePartitionKey,
                            RowKey = WelcomeMessageRowKey,
                            Data = updatedData
                        };
                        break;

                    case Constants.StaticTabEntityType:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = StaticTabPartitionKey,
                            RowKey = StaticTabRowKey,
                            Data = updatedData
                        };
                        break;

                    case Constants.TicketEntityType:
                        ticketEntity.PartitionKey = TicketPartitionKey;
                        ticketEntity.RowKey = Guid.NewGuid().ToString();
                        entity = ticketEntity;
                        break;

                    default:
                        break;
                }

                var result = await this.StoreOrUpdateEntityAsync(entity);

                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetSavedEntityDetailAsync(string entityType)
        {
            try
            {
                await this.EnsureInitializedAsync();
                TableOperation searchOperation = null;
                switch (entityType)
                {
                    case Constants.TeamEntityType:
                        searchOperation = TableOperation.Retrieve<ConfigurationEntity>(TeamPartitionKey, TeamRowKey);
                        break;

                    case Constants.KnowledgeBaseEntityType:
                        searchOperation = TableOperation.Retrieve<ConfigurationEntity>(KnowledgeBasePartitionKey, KnowledgeBaseRowKey);
                        break;

                    case Constants.WelcomeMessageEntityType:
                        searchOperation = TableOperation.Retrieve<ConfigurationEntity>(WelcomeMessagePartitionKey, WelcomeMessageRowKey);
                        break;

                    case Constants.StaticTabEntityType:
                        searchOperation = TableOperation.Retrieve<ConfigurationEntity>(StaticTabPartitionKey, StaticTabRowKey);
                        break;

                    default:
                        break;
                }

                TableResult searchResult = await this.configurationCloudTable.ExecuteAsync(searchOperation);
                var result = (ConfigurationEntity)searchResult.Result;

                return string.IsNullOrEmpty(result?.Data) ? string.Empty : result.Data;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Store or update configuration entity in table storage
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(dynamic entity)
        {
            await this.EnsureInitializedAsync();
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            if (entity.GetType() == new ConfigurationEntity().GetType())
            {
                return await this.configurationCloudTable.ExecuteAsync(addOrUpdateOperation);
            }
            else
            {
                return await this.ticketCloudTable.ExecuteAsync(addOrUpdateOperation);
            }
        }

        /// <summary>
        /// Create teams table if it doesnt exists
        /// </summary>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.configurationCloudTable = cloudTableClient.GetTableReference(StorageInfo.ConfigurationTableName);
            this.ticketCloudTable = cloudTableClient.GetTableReference(StorageInfo.TicketTableName);

            await this.configurationCloudTable.CreateIfNotExistsAsync();
            await this.ticketCloudTable.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table
        /// </summary>
        /// <returns>Task</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }

        /// <summary>
        /// Based on deep link URL received find team id and return it to that it can be saved
        /// </summary>
        /// <param name="teamIdDeepLink">team Id deep link</param>
        /// <returns>team Id as string</returns>
        private string ExtractTeamIdFromDeepLink(string teamIdDeepLink)
        {
            int startEscapedIndex = teamIdDeepLink.IndexOf(TeamIdEscapedStartString);
            int endEscapedIndex = teamIdDeepLink.IndexOf(TeamIdEscapedEndString);

            int startUnescapedIndex = teamIdDeepLink.IndexOf(TeamIdUnescapedStartString);
            int endUnescapedIndex = teamIdDeepLink.IndexOf(TeamIdUnescapedEndString);

            string teamID = string.Empty;

            if (startEscapedIndex > -1 && endEscapedIndex > -1)
            {
                teamID = HttpUtility.UrlDecode(teamIdDeepLink.Substring(startEscapedIndex, endEscapedIndex - startEscapedIndex + TeamIdEscapedEndString.Length));
            }
            else if (startUnescapedIndex > -1 && endUnescapedIndex > -1)
            {
                teamID = teamIdDeepLink.Substring(startUnescapedIndex, endUnescapedIndex - startUnescapedIndex + TeamIdUnescapedEndString.Length);
            }

            return teamID;
        }
    }
}
