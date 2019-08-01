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
    using Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json;

    /// <summary>
    ///  This Class will be invoked by message extenion bot and will return result which will
    ///  be used for populating message extension
    /// </summary>
    public class MessagingExtension
    {
        private const int TextTrimLengthForThumbnailCard = 45;
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
                            ComposeExtension = await this.GetSearchResultAsync(searchQuery, turnContext.Activity.From.Name,  messageExtensionQuery.CommandId, messageExtensionQuery.QueryOptions.Count, messageExtensionQuery.QueryOptions.Skip),
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
        /// Get the results from Azure search service and populate the result (card + preview).
        /// </summary>
        /// <param name="query">query which the user had typed in message extension search.</param>
        /// <param name="requesterName">name of the requester requesting for results.</param>
        /// <param name="commandId">commandId to determine which tab in message extension has been invoked.</param>
        /// <param name="count">count for pagination.</param>
        /// <param name="skip">skip for pagination.</param>
        /// <returns><see cref="Task"/> returns MessagingExtensionResult which will be used for providing the card.</returns>
        public async Task<MessagingExtensionResult> GetSearchResultAsync(string query, string requesterName, string commandId, int? count, int? skip)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            IList<TicketEntity> searchServiceResults = new List<TicketEntity>();

            // Enable prefix matches
            query = (query ?? string.Empty) + "*";

            // commandId should be equal to Id mentioned in Manifest file under composeExtensions section
            switch (commandId)
            {
                case "recents":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.RecentTickets, query, count, skip);
                    break;

                case "openrequests":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.OpenTickets, query, count, skip);
                    break;

                case "assignedrequests":
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.AssignedTickets, query, count, skip);
                    break;
            }

            foreach (var searchResult in searchServiceResults)
            {
                var formattedResultTextList = this.FormatSubTextForThumbnailCard(searchResult, requesterName);
                ThumbnailCard previewCard = new ThumbnailCard
                {
                    Title = searchResult.Title,
                    Text = formattedResultTextList
                };

                var selectedTicketAdaptiveCard = new MessagingExtensionTicketsCard(searchResult);
                composeExtensionResult.Attachments.Add(selectedTicketAdaptiveCard.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// This will format the text according to the card type which needs to be displayed in messaging extension.
        /// </summary>
        /// <param name="searchResult">searchResult from Azure search service.</param>
        /// <param name="requesterName">name of the requester requesting for results.</param>
        /// <returns>returns string which will be displayed in messaging extension thumbnail card.</returns>
        private string FormatSubTextForThumbnailCard(TicketEntity searchResult, string requesterName)
        {
            StringBuilder resultSubText = new StringBuilder();
            resultSubText.Append("<div>");
            string thumbNailCardSecondLineText = this.GetDateAndTicketStatus(searchResult, requesterName);
            resultSubText.Append(this.TrimExceedingTextLength(thumbNailCardSecondLineText));
            resultSubText.Append("</div>");

            resultSubText.Append("<div>");
            if (!string.IsNullOrEmpty(searchResult.RequesterName))
            {
                resultSubText.Append(this.TrimExceedingTextLength(searchResult.RequesterName));
            }

            resultSubText.Append("</div>");

            return resultSubText.ToString();
        }

        /// <summary>
        /// This will check if trim is required based on text length and if its required then
        /// trim the text which needs to be displayed in thumbnail card.
        /// </summary>
        /// <param name="thumbNailCardText">the string which needs to be trimmed</param>
        /// <returns>returns trimmd or not trimmed string which will be displayed in messaging extension thumbnail card.</returns>
        private string TrimExceedingTextLength(string thumbNailCardText)
        {
            if (thumbNailCardText.Length > TextTrimLengthForThumbnailCard)
            {
                thumbNailCardText = thumbNailCardText.Substring(0, TextTrimLengthForThumbnailCard) + "...";
            }

            return thumbNailCardText;
        }

        /// <summary>
        /// This will get date and ticket status to be dispalyed in second line of thumbnail card in messaging extension.
        /// </summary>
        /// <param name="searchResult">searchResult from Azure search service.</param>
        /// <param name="requesterName">name of the requester requesting for results.</param>
        /// <returns>returns string which will be used in messaging extension.</returns>
        private string GetDateAndTicketStatus(TicketEntity searchResult, string requesterName)
        {
            StringBuilder dateAndStatus = new StringBuilder();
            if (searchResult.Status == (int)TicketState.Open && string.IsNullOrEmpty(searchResult.AssignedToName))
            {
                if (searchResult.DateCreated != null)
                {
                    dateAndStatus.Append(searchResult.DateCreated);
                }

                dateAndStatus.Append(" | ");

                if (searchResult.LastModifiedByName.Equals(requesterName))
                {
                    dateAndStatus.Append("Open");
                }
                else
                {
                    dateAndStatus.Append("Opened by " + searchResult.LastModifiedByName);
                }
            }
            else if (searchResult.Status == (int)TicketState.Open && !string.IsNullOrEmpty(searchResult.AssignedToName))
            {
                if (searchResult.DateAssigned != null)
                {
                    dateAndStatus.Append(searchResult.DateAssigned);
                }

                dateAndStatus.Append(" | ");

                if (!string.IsNullOrEmpty(searchResult.AssignedToName))
                {
                    dateAndStatus.Append("Assigned to " + searchResult.AssignedToName);
                }
            }
            else
            {
                if (searchResult.DateClosed != null)
                {
                    dateAndStatus.Append(searchResult.DateClosed);
                }

                dateAndStatus.Append(" | ");

                if (!string.IsNullOrEmpty(searchResult.AssignedToName))
                {
                    dateAndStatus.Append("Closed by " + searchResult.LastModifiedByName);
                }
            }

            return dateAndStatus.ToString();
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
