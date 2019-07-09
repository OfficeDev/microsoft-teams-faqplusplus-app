// <copyright file="ConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

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

        private static readonly string ConfigurationTableName = StorageInfo.ConfigurationTableName;

        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;
        private readonly HttpClient httpClient;
        private readonly string qnaMakerSubscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public ConfigurationProvider(HttpClient httpClient, string qnaMakerSubscriptionKey, string connectionString)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.qnaMakerSubscriptionKey = qnaMakerSubscriptionKey;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveOrUpdateEntityAsync(string updatedData, string entityType)
        {
            try
            {
                ConfigurationEntity entity = null;
                switch (entityType)
                {
                    case Constants.Teams:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = TeamPartitionKey,
                            RowKey = TeamRowKey,
                            Data = updatedData
                        };
                        break;
                    case Constants.KnowledgeBase:
                        entity = new ConfigurationEntity()
                        {
                            PartitionKey = KnowledgeBasePartitionKey,
                            RowKey = KnowledgeBaseRowKey,
                            Data = updatedData
                        };
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
                GetKnowledgeBaseDetailsResponse kbDetails = await this.GetKnowledgeBaseDetailsAsync(knowledgeBaseId);

                if (kbDetails != null && kbDetails.Id.Equals(knowledgeBaseId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetSavedEntityDetailAsync(string entityType)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigurationTableName);

            TableOperation searchOperation = null;

            switch (entityType)
            {
                case Constants.Teams:
                    searchOperation = TableOperation.Retrieve<ConfigurationEntity>(TeamPartitionKey, TeamRowKey);
                    break;

                case Constants.KnowledgeBase:
                    searchOperation = TableOperation.Retrieve<ConfigurationEntity>(KnowledgeBasePartitionKey, KnowledgeBaseRowKey);
                    break;
                default:
                    break;
            }

            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);

            var result = (ConfigurationEntity)searchResult.Result;

            return string.IsNullOrEmpty(result?.Data) ? string.Empty : result.Data;
        }

        /// <inheritdoc/>
        public async Task<GetKnowledgeBaseDetailsResponse> GetKnowledgeBaseDetailsAsync(string kbId)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/{kbId}";
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.qnaMakerSubscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<GetKnowledgeBaseDetailsResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Store or update configuration entity in table storage
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(ConfigurationEntity entity)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigurationTableName);
            cloudTable.CreateIfNotExists();
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);

            return await cloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
