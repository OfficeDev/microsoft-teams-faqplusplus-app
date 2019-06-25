// <copyright file="KnowledgeBaseHelper.cs" company="Microsoft">
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

    /// <summary>
    /// Knowledge base helper.
    /// </summary>
    public class KnowledgeBaseHelper
    {
        private const string PartitionKey = "KnowledgeBaseInfo";
        private const string RowKey = "KnowledgeBaseId";

        private static readonly string KnowledgeBaseTableName = StorageInfo.KnowledgeBaseTableName;
        private readonly Lazy<Task> initializeTask;
        private CloudStorageAccount storageAccount;
        private CloudTableClient cloudTableClient;
        private CloudTable cloudTable;
        private HttpClient httpClient;
        private string qnaMakerSubcriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Http client to be used.</param>
        /// /// <param name="qnaMakerSubscriptionKey">QnAMaker subscription key</param>
        /// <param name="connectionString">connection string of storage.</param>
        public KnowledgeBaseHelper(HttpClient httpClient, string qnaMakerSubscriptionKey, string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(httpClient, qnaMakerSubscriptionKey, connectionString));
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

            var result = await this.StoreOrUpdateKnowledgeBaseEntityAsync(knowledgeBaseEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Get already saved knowledge base Id from storage table
        /// </summary>
        /// <returns><see cref="Task"/> Already saved knowledge base Id.</returns>
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            await this.EnsureInitializedAsync();

            TableOperation searchOperation = TableOperation.Retrieve<KnowledgeBaseEntity>(PartitionKey, RowKey);
            TableResult searchResult = await this.cloudTable.ExecuteAsync(searchOperation);

            var result = (KnowledgeBaseEntity)searchResult.Result;

            return string.IsNullOrEmpty(result?.KnowledgeBaseId) ? string.Empty : result.KnowledgeBaseId;
        }

        /// <summary>
        /// Validate from QnA Service if the passed knowledgebase Id is valid or not
        /// </summary>
        /// <param name="knowledgeBaseId">knowledge base Id received from view page</param>
        /// <returns><see cref="Task"/> boolean value indicating knowledgebase Id is valid or not.</returns>
        public async Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId)
        {
            await this.EnsureInitializedAsync();

            QnAMakerService qnAMakerService = new QnAMakerService(this.httpClient, this.qnaMakerSubcriptionKey);
            GetKnowledgeBaseDetailsResponse kbDetails = await qnAMakerService.GetKnowledgeBaseDetailsAsync(knowledgeBaseId);

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
        private async Task<TableResult> StoreOrUpdateKnowledgeBaseEntityAsync(KnowledgeBaseEntity knowledgeBaseEntity)
        {
            await this.EnsureInitializedAsync();

            TableOperation addOrUpdateOperation = TableOperation.InsertOrMerge(knowledgeBaseEntity);

            return await this.cloudTable.ExecuteAsync(addOrUpdateOperation);
        }

        /// <summary>
        /// Create knowledge base table if it doesnt exists
        /// </summary>
        /// <param name="httpClient">http client from the constrcutor</param>
        /// <param name="qnaMakerSubscriptionKey">qna maker subscription key from the configuraton file</param>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(HttpClient httpClient, string qnaMakerSubscriptionKey, string connectionString)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.qnaMakerSubcriptionKey = qnaMakerSubscriptionKey;

            this.cloudTable = this.cloudTableClient.GetTableReference(KnowledgeBaseTableName);
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
    }
}
