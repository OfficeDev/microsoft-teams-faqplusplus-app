// <copyright file="KnowledgeBaseHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Knowledge base helper.
    /// </summary>
    public class KnowledgeBaseHelper
    {
        private const string PartitionKey = "KnowledgeBaseInfo";
        private const string RowKey = "KnowledgeBaseId";

        private static readonly string KnowledgeBaseTableName = StorageInfo.KnowledgeBaseTableName;
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;
        private readonly HttpClient httpClient;
        private readonly string qnaMakerSubcriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        /// <param name="connectionString">connection string of storage.</param>
        public KnowledgeBaseHelper(HttpClient httpClient, string qnaMakerSubscriptionKey, string connectionString)
        {
            this.httpClient = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.qnaMakerSubcriptionKey = qnaMakerSubscriptionKey;
        }

        /// <summary>
        /// Save or update knowledge base Id.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledge base Id received from view page</param>
        /// <returns><see cref="Task"/> boolean value that represents if knowledge base Id is saved or updated.</returns>
        public async Task<bool> SaveOrUpdateKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            KnowledgeBaseEntity knowledgeBaseEntity = new KnowledgeBaseEntity()
            {
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                KnowledgeBaseId = knowledgeBaseId
            };

            var result = await this.StoreOrUpdateKnowledgeBaseEntity(knowledgeBaseEntity);

            if (result.HttpStatusCode != (int)HttpStatusCode.NoContent)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get already saved knowledge base Id from storage table
        /// </summary>
        /// <returns><see cref="Task"/> Already saved knowledge base Id.</returns>
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(KnowledgeBaseTableName);
            TableOperation searchOperation = TableOperation.Retrieve<KnowledgeBaseEntity>(PartitionKey, RowKey);
            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);

            var result = (KnowledgeBaseEntity)searchResult.Result;
            string knowledgeBaseId = string.IsNullOrEmpty(result?.KnowledgeBaseId) ? string.Empty : result.KnowledgeBaseId;

            return knowledgeBaseId;
        }

        /// <summary>
        /// Validate from QnA Service if the passed knowledgebase Id is valid or not
        /// </summary>
        /// <param name="knowledgeBaseId">knowledge base Id received from view page</param>
        /// <returns><see cref="Task"/> boolean value indicating knowledgebase Id is valid or not.</returns>
        public async Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId)
        {
            QnAMakerService qnAMakerService = new QnAMakerService(this.httpClient, this.qnaMakerSubcriptionKey);
            GetKnowledgeBaseDetailsResponse kbDetails = await qnAMakerService.GetKnowledgeBaseDetails(knowledgeBaseId);

            if (kbDetails != null && kbDetails.Id.Equals(knowledgeBaseId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Store or update knowledge base entity in table storage
        /// </summary>
        /// <param name="knowledgeBaseEntity">knowledgeBaseEntity.</param>
        /// <returns><see cref="Task"/> that represents knowledge base id is saved or updated.</returns>
        private Task<TableResult> StoreOrUpdateKnowledgeBaseEntity(KnowledgeBaseEntity knowledgeBaseEntity)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(KnowledgeBaseTableName);
            cloudTable.CreateIfNotExists();
            TableOperation addOrUpdateOperation = TableOperation.InsertOrMerge(knowledgeBaseEntity);

            return cloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
