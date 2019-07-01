// <copyright file="ConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
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
        private const string TeamPartitionKey = "TeamInfo";
        private const string TeamRowKey = "MSTeamId";

        private static readonly string ConfigurationTableName = StorageInfo.ConfigurationTableName;
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public ConfigurationProvider(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = this.storageAccount.CreateCloudTableClient();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveOrUpdateTeamIdAsync(string teamId)
        {
            ConfigurationEntity teamEntity = new ConfigurationEntity()
            {
                PartitionKey = TeamPartitionKey,
                RowKey = TeamRowKey,
                TeamId = teamId
            };

            var result = await this.StoreOrUpdateTeamEntityAsync(teamEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <inheritdoc/>
        public async Task<string> GetSavedTeamIdAsync()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigurationTableName);
            TableOperation searchOperation = TableOperation.Retrieve<ConfigurationEntity>(TeamPartitionKey, TeamRowKey);
            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);

            var result = (ConfigurationEntity)searchResult.Result;
            string teamId = string.IsNullOrEmpty(result?.TeamId) ? string.Empty : result.TeamId;

            return teamId;
        }

        /// <summary>
        /// Store or update team entity in table storage
        /// </summary>
        /// <param name="teamEntity">Team entity.</param>
        /// <returns><see cref="Task"/> that represents team id is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateTeamEntityAsync(ConfigurationEntity teamEntity)
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigurationTableName);
            cloudTable.CreateIfNotExists();
            TableOperation addorUpdateOperation = TableOperation.InsertOrMerge(teamEntity);

            return await cloudTable.ExecuteAsync(addorUpdateOperation);
        }
    }
}
