// <copyright file="ConfigurationProvider.cs" company="Microsoft">
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
            try
            {
                ConfigurationEntity teamEntity = new ConfigurationEntity()
                {
                    PartitionKey = TeamPartitionKey,
                    RowKey = TeamRowKey,
                    Data = teamId
                };

                var result = await this.StoreOrUpdateEntityAsync(teamEntity);

                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
    }

        /// <inheritdoc/>
        public async Task<string> GetSavedTeamIdAsync()
        {
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(ConfigurationTableName);
            TableOperation searchOperation = TableOperation.Retrieve<ConfigurationEntity>(TeamPartitionKey, TeamRowKey);
            TableResult searchResult = await cloudTable.ExecuteAsync(searchOperation);

            var result = (ConfigurationEntity)searchResult.Result;
            string teamId = string.IsNullOrEmpty(result?.Data) ? string.Empty : result.Data;

            return teamId;
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
