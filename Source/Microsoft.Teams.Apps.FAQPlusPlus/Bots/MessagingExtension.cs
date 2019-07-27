// <copyright file="MessagingExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json;

    /// <summary>
    ///  This Class will be invoked by m essage extenion bot and will return result which will
    ///  be used for populating message extension
    /// </summary>
    public class MessagingExtension
    {
        private const int TextTrimLengthForCard = 10;
        private const string ManifestExtensionParameter = "searchText"; // searchText is the parameter name in the manifest file
        private readonly ISearchService searchService;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtension"/> class.
        /// </summary>
        /// <param name="searchService">searchService DI.</param>
        /// <param name="telemetryClient">telemetryClient DI.</param>
        public MessagingExtension(ISearchService searchService, TelemetryClient telemetryClient)
        {
            this.searchService = searchService;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Based on type of activity return the search results or error result.
        /// </summary>
        /// <param name="turnContext">turnContext for messaging extension.</param>
        /// <returns><see cref="Task"/> returns invokeresponse which will be used for providing the search result.</returns>
        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            try
            {
                if (turnContext.Activity.Name == "composeExtension/query")
                {
                    var messageExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
                    var searchQuery = this.GetSearchQueryString(messageExtensionQuery);

                    return new InvokeResponse
                    {
                        Body = new MessagingExtensionResponse
                        {
                            ComposeExtension = await this.GetSearchResultAsync(searchQuery, messageExtensionQuery.CommandId, messageExtensionQuery.QueryOptions.Skip, messageExtensionQuery.QueryOptions.Count),
                        },
                        Status = 200,
                    };
                }
                else
                {
                    InvokeResponse response = null;
                    return response;
                }
            }
            catch (Exception error)
            {
                this.telemetryClient.TrackTrace($"Failed to compose a list for messaging extension: {error.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(error);
                throw;
            }
        }

        /// <summary>
        /// Get the results from Azure search service and populate the preview as well as card.
        /// </summary>
        /// <param name="query">query which the user had typed in message extension search.</param>
        /// <param name="commandId">commandId to determine which tab in message extension has been invoked.</param>
        /// <param name="skip">skip for pagination.</param>
        /// <param name="count">count for pagination.</param>
        /// <returns><see cref="Task"/> returns MessagingExtensionResult which will be used for providing the card.</returns>
        public async Task<MessagingExtensionResult> GetSearchResultAsync(string query, string commandId, int? skip, int? count)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            IList<TicketEntity> searchServiceResults = null;

            // commandId should be equal to Id mentioned in Manifet file under composeExtensions section
            switch (commandId)
            {
                case "recents":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.RecentTickets, query, count, skip);
                    break;

                case "openrequests":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.OpenTickets, query);
                    break;

                case "assignedrequests":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.AssignedTickets, query);
                    break;
            }

            foreach (var searchResult in searchServiceResults)
            {
                var formattedResultTextForPreview = this.FormatSubTextForThumbnailCard(searchResult, true);
                ThumbnailCard previewCard = new ThumbnailCard
                {
                    Title = searchResult.AssignedTo,
                    Text = formattedResultTextForPreview,
                };

                var formattedResultTextForCard = this.FormatSubTextForThumbnailCard(searchResult, false);
                ThumbnailCard card = new ThumbnailCard
                {
                    Title = searchResult.AssignedTo,
                    Text = formattedResultTextForCard,
                };

                composeExtensionResult.Attachments.Add(card.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// This will format the text according to the card type which needs to be displayed in messaging extension.
        /// </summary>
        /// <param name="searchResult">searchResult from Azure search service.</param>
        /// <param name="isPreview">to determine if the formatting is for preview or card.</param>
        /// <returns>returns string which will be used in messaging extension.</returns>
        private string FormatSubTextForThumbnailCard(TicketEntity searchResult, bool isPreview)
        {
            StringBuilder resultSubText = new StringBuilder();
            if (!string.IsNullOrEmpty(searchResult.Text))
            {
                if (searchResult.Text.Length > TextTrimLengthForCard && isPreview)
                {
                    resultSubText.Append("Request: " + searchResult.Text.Substring(0, TextTrimLengthForCard) + "...");
                }
                else
                {
                    resultSubText.Append("Request: " + searchResult.Text);
                }
            }

            if (searchResult.Status == (int)TicketState.Open)
            {
                resultSubText.Append(" | " + TicketState.Open);
            }
            else
            {
                resultSubText.Append(" | " + TicketState.Closed);
            }

            if (searchResult.DateCreated != null)
            {
                resultSubText.Append(" | " + searchResult.DateCreated);
            }

            return resultSubText.ToString();
        }

        /// <summary>
        /// Returns query which the user has typed in message extension search.
        /// </summary>
        /// <param name="query">query typed by user in message extension.</param>
        /// <returns> returns user typed query.</returns>
        private string GetSearchQueryString(MessagingExtensionQuery query)
        {
            string messageExtensionInputText = string.Empty;
            foreach (var parameter in query.Parameters)
            {
                if (parameter.Name.Equals(ManifestExtensionParameter, StringComparison.OrdinalIgnoreCase))
                {
                    messageExtensionInputText = parameter.Value.ToString();
                    break;
                }
            }

            return messageExtensionInputText;
        }
    }
}
