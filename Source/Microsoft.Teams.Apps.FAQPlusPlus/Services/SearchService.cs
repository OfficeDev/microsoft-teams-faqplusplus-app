// <copyright file="SearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;

    /// <summary>
    /// SearchService which will help in creating index, indexer and datasource if it doesn't exists
    /// for indexing table which will be used for search by message extension.
    /// </summary>
    public class SearchService : ISearchService
    {
        private const string TicketsIndexName = "tickets-index";
        private const string TicketsIndexerName = "tickets-indexer";
        private const string TicketsDataSourceName = "tickets-storage";

        private const int TicketSearchPaginationTop = 10;
        private const int TicketSearchPaginationSkip = 0;
        private const bool IsTotalCountIncluded = false;

        private readonly Lazy<Task<bool>> initializeTask;
        private readonly IConfiguration configuration;
        private readonly TelemetryClient telemetryClient;
        private SearchServiceClient searchServiceClient;
        private SearchIndexClient searchIndexClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration provided by DI</param>
        /// <param name="telemetryClient">TelemetryClient provided by DI</param>
        public SearchService(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            this.configuration = configuration;
            this.telemetryClient = telemetryClient;
            this.initializeTask = new Lazy<Task<bool>>(() => this.InitializeAsync());
        }

        /// <inheritdoc/>
        public async Task<IList<TicketEntity>> SMESearchServiceForMessageExtension(TicketSearchScope searchScope, string searchQuery)
        {
            bool isTicketIndexingServiceCreated = await this.EnsureInitializedAsync();
            IList<TicketEntity> ticketList = null;
            if (isTicketIndexingServiceCreated)
            {
                SearchParameters searchParam = new SearchParameters();
                switch (searchScope)
                {
                    case TicketSearchScope.RecentTickets:
                        searchParam.OrderBy = new[] { "DateAssigned asc" };
                        break;

                    case TicketSearchScope.OpenTickets:
                        searchParam.Filter = "Status eq 0";
                        searchParam.OrderBy = new[] { "DateCreated asc" };
                        break;

                    case TicketSearchScope.AssignedTickets:
                        searchParam.Filter = "AssignedTo ne ' '";
                        searchParam.OrderBy = new[] { "DateAssigned asc" };
                        break;

                    default:
                        break;
                }

                searchParam.Top = TicketSearchPaginationTop;
                searchParam.Skip = TicketSearchPaginationSkip;
                searchParam.IncludeTotalResultCount = IsTotalCountIncluded;
                searchParam.Select = new[] { "TicketId", "Text", "Status", "AssignedTo", "DateCreated" };

                var docs = await this.searchIndexClient.Documents.SearchAsync<TicketEntity>(searchQuery, searchParam);

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
            }

            return ticketList;
        }

        /// <summary>
        /// Create index, indexer and data source it doesnt exists
        /// </summary>
        /// <returns><see cref="bool"/> representing the boolean task which represents index, indxer and datasource is created if its not existing.</returns>
        private async Task<bool> InitializeAsync()
        {
            try
            {
                this.searchServiceClient = new SearchServiceClient(
                this.configuration["SearchServiceName"],
                new SearchCredentials(this.configuration["SearchServiceAdminApiKey"]));

                this.searchIndexClient = new SearchIndexClient(this.configuration["SearchServiceName"], TicketsIndexName, new SearchCredentials(this.configuration["SearchServiceQueryApiKey"]));

                await this.CreateIndexAsync();
                await this.CreateDataSourceAsync(this.configuration["StorageConnectionString"]);
                await this.CreateIndexerAsync();

                return true;
            }
            catch (Exception error)
            {
                this.telemetryClient.TrackException(error);
                return false;
            }
        }

        /// <summary>
        /// Create index in Azure search service if it doesn't exists
        /// </summary>
        /// <returns><see cref="Task"/> that represents index is created if it is not created.</returns>
        private async Task CreateIndexAsync()
        {
            if (!this.searchServiceClient.Indexes.Exists(TicketsIndexName))
            {
                var tableIndex = new Index()
                {
                    Name = TicketsIndexName,
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
        private async Task CreateDataSourceAsync(string connectionString)
        {
            if (!this.searchServiceClient.DataSources.Exists(TicketsDataSourceName))
            {
                var dataSource = DataSource.AzureTableStorage(
                                  name: TicketsDataSourceName,
                                  storageConnectionString: connectionString,
                                  tableName: StorageInfo.TicketTableName);

                await this.searchServiceClient.DataSources.CreateAsync(dataSource);
            }
        }

        /// <summary>
        /// Create indexer if it doesnt exists in Azure search service
        /// </summary>
        /// <returns><see cref="Task"/> that represents indexer is created if not available in Azure search service.</returns>
        private async Task CreateIndexerAsync()
        {
            if (!this.searchServiceClient.Indexers.Exists(TicketsIndexerName))
            {
                var indexer =
                new Indexer()
                {
                    Name = TicketsIndexerName,
                    DataSourceName = TicketsDataSourceName,
                    TargetIndexName = TicketsIndexName,
                    Schedule = new IndexingSchedule(TimeSpan.FromHours(1))
                };

                await this.searchServiceClient.Indexers.CreateAsync(indexer);
            }
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in indexing
        /// </summary>
        /// <returns>Task</returns>
        private async Task<bool> EnsureInitializedAsync()
        {
            return await this.initializeTask.Value;
        }
    }
}