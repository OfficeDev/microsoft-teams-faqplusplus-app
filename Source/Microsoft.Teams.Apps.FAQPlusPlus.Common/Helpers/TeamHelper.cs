// <copyright file="TeamHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
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
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamHelper"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage.</param>
        public TeamHelper(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
        }

        /// <summary>
        /// Store Team Id.
        /// </summary>
        /// <param name="teamId">Team Id.</param>
        /// <returns><see cref="Task"/> that represents if team id is saved or not.</returns>
        public async Task<bool> SaveTeamIdDetailAsync(string teamId)
        {
            TeamEntity teamEntity = new TeamEntity()
            {
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                TeamId = teamId
            };

            // Since one team Id will be stored. Check if team id is already added or not
            string getSavedTeamId = await this.GetSavedTeamIdAsync();
            if (getSavedTeamId == string.Empty)
            {
                var result = await this.StoreTeamEntity(teamEntity);

                if (result.HttpStatusCode != (int)System.Net.HttpStatusCode.NoContent)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get saved Team Id.
        /// </summary>
        /// <returns><see cref="Task"/> Saved team Id.</returns>
        public async Task<string> GetSavedTeamIdAsync()
        {
            string teamId = string.Empty;
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(TeamTableName);
            TableOperation searchOperation = TableOperation.Retrieve<TeamEntity>(PartitionKey, RowKey);
            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);
            var result = (TeamEntity)searchResult.Result;
            if (result != null && !string.IsNullOrEmpty(result.TeamId))
            {
                teamId = result.TeamId;
            }

            return teamId;
        }

        /// <summary>
        /// Store TeamEntity.
        /// </summary>
        /// <param name="teamEntity">Team entity.</param>
        /// <returns><see cref="Task"/> that represents store team function.</returns>
        private Task<TableResult> StoreTeamEntity(TeamEntity teamEntity)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(TeamTableName);
            cloudTable.CreateIfNotExists();
            TableOperation addOperation = TableOperation.InsertOrMerge(teamEntity);
            return cloudTable.ExecuteAsync(addOperation);
        }
    }
}
