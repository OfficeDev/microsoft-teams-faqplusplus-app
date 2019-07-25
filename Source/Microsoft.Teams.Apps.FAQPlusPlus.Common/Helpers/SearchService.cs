// <copyright file="SearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// SearchService which will help in creating index, indexer and datasource if it doesn't exists
    /// for indexing table which will be used for search by message extension.
    /// </summary>
    public class SearchService : ISearchService
    {
        private const string SearchIndexName = "tickets-index";
        private const string SearchIndexerName = "tickets-indexer";
        private const string SearchDataSourceName = "tickets-storage";

        private readonly Lazy<Task> initializeTask;
        private SearchServiceClient searchServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        /// <param name="searchServiceName">search service name of Azure search services provided by DI</param>
        /// <param name="searchServiceAdminApiKey">search service admin api key of Azure search service by DI</param>
        public SearchService(string connectionString, string searchServiceName, string searchServiceAdminApiKey)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString, searchServiceName, searchServiceAdminApiKey));
        }

        /// <inheritdoc/>
        public async Task<List<TicketEntity>> SMESearchServiceForMessageExtension(string searchQuery, string tabType)
        {
            await this.EnsureInitializedAsync();
            List<TicketEntity> ticketList = new List<TicketEntity>();

            ISearchIndexClient indexClient = this.searchServiceClient.Indexes.GetClient(SearchIndexName);
            SearchParameters searchParam = null;
            switch (tabType)
            {
                case MessagingExtensionConstants.RecentTabType:
                    searchParam = new SearchParameters()
                    {
                        OrderBy = new[] { "DateAssigned asc" },
                        Select = new[] { "TicketId", "Text", "Status", "AssignedTo", "DateCreated" }
                    };
                    break;

                case MessagingExtensionConstants.OpenTabType:
                    searchParam = new SearchParameters()
                    {
                        Filter = "Status eq 0",
                        OrderBy = new[] { "DateCreated asc" },
                        Select = new[] { "TicketId", "Text", "Status", "AssignedTo", "DateCreated" }
                    };
                    break;

                case MessagingExtensionConstants.AssignedTabType:
                    searchParam = new SearchParameters()
                    {
                        Filter = "AssignedTo ne ' '",
                        OrderBy = new[] { "DateAssigned asc" },
                        Select = new[] { "TicketId", "Text", "Status", "AssignedTo", "DateCreated" }
                    };
                    break;

                default:
                    break;
            }

            var docs = await indexClient.Documents.SearchAsync<TicketEntity>(searchQuery, searchParam);

            if (docs != null)
            {
                foreach (SearchResult<TicketEntity> doc in docs.Results)
                {
                    ticketList.Add(new TicketEntity
                    {
                        RowKey = doc.Document.TicketId,
                        Text = doc.Document.Text,
                        Status = doc.Document.Status,
                        AssignedTo = doc.Document.AssignedTo,
                        DateCreated = doc.Document.DateCreated
                    });
                }
            }

            return ticketList;
        }

        /// <summary>
        /// Create index, indexer and data source it doesnt exists
        /// </summary>
        /// <param name="connectionString">storage account connection string</param>
        /// <param name="searchServiceName">Azure search service name provided by DI</param>
        /// <param name="searchServiceAdminApiKey">Azure search service api key provided by DI</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents index, indxer and datasource is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString, string searchServiceName, string searchServiceAdminApiKey)
        {
            this.searchServiceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceAdminApiKey));

            try
            {
                await this.SearchServiceCreateIndexAsync();
                await this.SyncDataFromStorageTableAsync(connectionString);
                await this.SearchServiceCreateIndexer();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Create index in Azure search service if it doesn't exists
        /// </summary>
        /// <returns><see cref="Task"/> that represents index is created if it is not created.</returns>
        private async Task SearchServiceCreateIndexAsync()
        {
            if (!this.searchServiceClient.Indexes.Exists(SearchIndexName))
            {
                var tableIndex = new Index()
                {
                    Name = SearchIndexName,
                    Fields = FieldBuilder.BuildForType<TicketEntity>()
                };
                await this.searchServiceClient.Indexes.CreateAsync(tableIndex);
            }
        }

        /// <summary>
        /// Add data source if it doesn't exists in Azure search service
        /// </summary>
        /// <param name="connectionString">connectionString.</param>
        /// <returns><see cref="Task"/> that represents data source is added to Azure search service.</returns>
        private async Task SyncDataFromStorageTableAsync(string connectionString)
        {
            try
            {
                if (!this.searchServiceClient.DataSources.Exists(SearchDataSourceName))
                {
                    var dataSource = DataSource.AzureTableStorage(
                                      name: SearchDataSourceName,
                                      storageConnectionString: connectionString,
                                      tableName: StorageInfo.TicketTableName);

                    await this.searchServiceClient.DataSources.CreateAsync(dataSource);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Create indexer if it doesnt exists in Azure search service
        /// </summary>
        /// <returns><see cref="Task"/> that represents indexer is created if not available in Azure search service.</returns>
        private async Task SearchServiceCreateIndexer()
        {
            if (!this.searchServiceClient.Indexers.Exists(SearchIndexerName))
            {
                var indexer =
                new Indexer()
                {
                    Name = SearchIndexerName,
                    DataSourceName = SearchDataSourceName,
                    TargetIndexName = SearchIndexName,
                    Schedule = new IndexingSchedule(TimeSpan.FromHours(1))
                };

                await this.searchServiceClient.Indexers.CreateAsync(indexer);
            }
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in indexing
        /// </summary>
        /// <returns>Task</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }
    }
}