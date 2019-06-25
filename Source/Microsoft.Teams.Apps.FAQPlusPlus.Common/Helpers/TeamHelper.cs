// <copyright file="TeamHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Team Helper.
    /// </summary>
    public class TeamHelper
    {
        private const string PartitionKey = "TeamInfo";
        private const string RowKey = "MSTeamId";

        private static readonly string TeamTableName = StorageInfo.TeamTableName;
        private readonly Lazy<Task> initializeTask;
        private CloudStorageAccount storageAccount;
        private CloudTableClient cloudTableClient;
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamHelper"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage.</param>
        public TeamHelper(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString));
        }

        /// <summary>
        /// Save or update team Id.
        /// </summary>
        /// <param name="teamId">Team Id received from view page</param>
        /// <returns><see cref="Task"/> boolean value that represents if team Id is saved or updated.</returns>
        public async Task<bool> SaveOrUpdateTeamIdAsync(string teamId)
        {
            TeamEntity teamEntity = new TeamEntity()
            {
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                TeamId = teamId
            };

            var result = await this.StoreOrUpdateTeamEntityAsync(teamEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Get already saved team Id from storage table
        /// </summary>
        /// <returns><see cref="Task"/> Already saved team Id.</returns>
        public async Task<string> GetSavedTeamIdAsync()
        {
            await this.EnsureInitializedAsync();

            TableOperation searchOperation = TableOperation.Retrieve<TeamEntity>(PartitionKey, RowKey);
            TableResult searchResult = await this.cloudTable.ExecuteAsync(searchOperation);

            var result = (TeamEntity)searchResult.Result;

            return string.IsNullOrEmpty(result?.TeamId) ? string.Empty : result.TeamId;
        }

        /// <summary>
        /// Store or update team entity in table storage
        /// </summary>
        /// <param name="teamEntity">Team entity.</param>
        /// <returns><see cref="Task"/> that represents team id is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateTeamEntityAsync(TeamEntity teamEntity)
        {
            await this.EnsureInitializedAsync();

            TableOperation addorUpdateOperation = TableOperation.InsertOrMerge(teamEntity);

            return await this.cloudTable.ExecuteAsync(addorUpdateOperation);
        }

        /// <summary>
        /// Create teams table if it doesnt exists
        /// </summary>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
            this.cloudTable = this.cloudTableClient.GetTableReference(TeamTableName);

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
