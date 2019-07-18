// <copyright file="ConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
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

        private const string TeamIdStartString = "19%3a";
        private const string TeamIdEndString = "%40thread.skype";

        private readonly Lazy<Task> initializeTask;
        private CloudTable cloudTable;
        private string qnaMakerSubscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public ConfigurationProvider(string qnaMakerSubscriptionKey, string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(qnaMakerSubscriptionKey, connectionString));
        }

        /// <inheritdoc/>
        public async Task<bool> SaveOrUpdateEntityAsync(string updatedData, string entityType)
        {
            try
            {
                ConfigurationEntity entity = null;
                switch (entityType)
                {
                    case Constants.TeamEntityType:
                        // Teams textbox in view will contain deeplink of one Teams from which
                        // team id will be extracted and stored in table
                        string teamIdTobeStored = this.ExtractTeamIdFromDeepLink(updatedData);
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = TeamPartitionKey,
                            RowKey = TeamRowKey,
                            Data = teamIdTobeStored
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
        public async Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId)
        {
            try
            {
                QnAMakerService qnAMakerService = new QnAMakerService(this.qnaMakerSubscriptionKey);
                KnowledgebaseDTO kbDetails = await qnAMakerService.GetKnowledgeBaseDetailsAsync(knowledgeBaseId);
                return kbDetails != null && kbDetails.Id.Equals(knowledgeBaseId);
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

                    default:
                        break;
                }

                TableResult searchResult = await this.cloudTable.ExecuteAsync(searchOperation);
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
        private async Task<TableResult> StoreOrUpdateEntityAsync(ConfigurationEntity entity)
        {
            await this.EnsureInitializedAsync();
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);

            return await this.cloudTable.ExecuteAsync(addOrUpdateOperation);
        }

        /// <summary>
        /// Create teams table if it doesnt exists
        /// </summary>
        /// <param name="qnaMakerSubscriptionKey">qna maker subscription key from the configuraton file</param>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string qnaMakerSubscriptionKey, string connectionString)
        {
            this.qnaMakerSubscriptionKey = qnaMakerSubscriptionKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.cloudTable = cloudTableClient.GetTableReference(StorageInfo.ConfigurationTableName);

            await this.cloudTable.CreateIfNotExistsAsync();
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
            int startIndex = teamIdDeepLink.IndexOf(TeamIdStartString);
            int endIndex = teamIdDeepLink.IndexOf(TeamIdEndString);

            return HttpUtility.UrlDecode(teamIdDeepLink.Substring(startIndex, endIndex - startIndex + TeamIdEndString.Length));
        }
    }
}
