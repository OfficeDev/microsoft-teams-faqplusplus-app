// <copyright file="MessagingExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the logic of the messaging extension for FAQ++
    /// </summary>
    public class MessagingExtension
    {
        private const int TextTrimLengthForThumbnailCard = 45;
        private const string SearchTextParameterName = "searchText";        // parameter name in the manifest file

        private readonly ISearchService searchService;
        private readonly TelemetryClient telemetryClient;
        private readonly IConfiguration configuration;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly ConversationReference conversationReference;
        private readonly Common.Providers.IConfigurationProvider configurationProvider;
        private readonly ConversationAccount conversationAccount;

        private string currentUserId;
        private bool isUserPartOfRoster = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtension"/> class.
        /// </summary>
        /// <param name="searchService">searchService DI.</param>
        /// <param name="telemetryClient">telemetryClient DI.</param>
        /// <param name="configuration">configuration DI.</param>
        /// <param name="adapter">adapter DI.</param>
        /// <param name="conversationReference">conversationReference DI.</param>
        /// <param name="configurationProvider">configurationProvider DI.</param>
        /// <param name="conversationAccount">conversationAccount DI.</param>
        public MessagingExtension(
            ISearchService searchService,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            IBotFrameworkHttpAdapter adapter,
            ConversationReference conversationReference,
            Common.Providers.IConfigurationProvider configurationProvider,
            ConversationAccount conversationAccount)
        {
            this.searchService = searchService;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
            this.adapter = adapter;
            this.conversationReference = conversationReference;
            this.configurationProvider = configurationProvider;
            this.conversationAccount = conversationAccount;
        }

        /// <summary>
        /// Based on type of activity return the search results or error result.
        /// </summary>
        /// <param name="turnContext">turnContext for messaging extension.</param>
        /// <returns><see cref="Task"/> that returns an <see cref="InvokeResponse"/> with search results, or null to ignore the activity.</returns>
        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            try
            {
                if (turnContext.Activity.Name == "composeExtension/query")
                {
                    if (await this.IsValidSME(turnContext))
                    {
                        var messageExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
                        var searchQuery = this.GetSearchQueryString(messageExtensionQuery);

                        return new InvokeResponse
                        {
                            Body = new MessagingExtensionResponse
                            {
                                ComposeExtension = await this.GetSearchResultAsync(searchQuery, messageExtensionQuery.CommandId, messageExtensionQuery.QueryOptions.Count, messageExtensionQuery.QueryOptions.Skip),
                            },
                            Status = 200,
                        };
                    }
                    else
                    {
                        return new InvokeResponse
                        {
                            Body = new MessagingExtensionResponse
                            {
                                ComposeExtension = new MessagingExtensionResult
                                {
                                    Text = "You don't have access to this app. Please contact your IT admin.",
                                    Type = "message"
                                },
                            },
                            Status = 200,
                        };
                    }
                }
                else
                {
                    InvokeResponse response = null;
                    return response;
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"Failed to handle the ME command {turnContext.Activity.Name}: {ex.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the results from Azure search service and populate the result (card + preview).
        /// </summary>
        /// <param name="query">query which the user had typed in message extension search.</param>
        /// <param name="commandId">commandId to determine which tab in message extension has been invoked.</param>
        /// <param name="count">count for pagination.</param>
        /// <param name="skip">skip for pagination.</param>
        /// <returns><see cref="Task"/> returns MessagingExtensionResult which will be used for providing the card.</returns>
        public async Task<MessagingExtensionResult> GetSearchResultAsync(string query, string commandId, int? count, int? skip)
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

            foreach (var ticket in searchServiceResults)
            {
                ThumbnailCard previewCard = new ThumbnailCard
                {
                    Title = ticket.Title,
                    Text = this.GetPreviewCardText(ticket),
                };

                var selectedTicketAdaptiveCard = new MessagingExtensionTicketsCard(ticket);
                composeExtensionResult.Attachments.Add(selectedTicketAdaptiveCard.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        // Get the text for the preview card for the result
        private string GetPreviewCardText(TicketEntity ticket)
        {
            var text = $@"
<div>
  <div style='white-space:nowrap'>{HttpUtility.HtmlEncode(ticket.DateCreated.ToShortDateString())} | {HttpUtility.HtmlEncode(this.GetDisplayStatus(ticket))}</div>
  <div style='white-space:nowrap'>{HttpUtility.HtmlEncode(ticket.RequesterName)}</div>
</div>";
            return text.Trim();
        }

        // Construct the string to display for the status of the ticket
        private string GetDisplayStatus(TicketEntity ticket)
        {
            switch (ticket.Status)
            {
                case (int)TicketState.Open:
                    return string.IsNullOrEmpty(ticket.AssignedToName) ?
                        Resource.OpenStatusValue :
                        string.Format(CultureInfo.CurrentCulture, Resource.AssignedToStatusValue, ticket.AssignedToName);

                case (int)TicketState.Closed:
                    return string.Format(CultureInfo.CurrentCulture, Resource.ClosedByStatusValue, ticket.LastModifiedByName);

                default:
                    this.telemetryClient.TrackTrace($"Unknown ticket status {ticket.Status}", ApplicationInsights.DataContracts.SeverityLevel.Warning);
                    return string.Empty;
            }
        }

        // Get the value of the searchText parameter in the ME query
        private string GetSearchQueryString(MessagingExtensionQuery query)
        {
            string messageExtensionInputText = string.Empty;
            foreach (var parameter in query.Parameters)
            {
                if (parameter.Name.Equals(SearchTextParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    messageExtensionInputText = parameter.Value.ToString();
                    break;
                }
            }

            return messageExtensionInputText;
        }

        // Check if user using the app is a valid SME or not
        private async Task<bool> IsValidSME(ITurnContext<IInvokeActivity> turnContext)
        {
            var teamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
            this.conversationAccount.Id = teamId;
            this.conversationReference.ServiceUrl = turnContext.Activity.ServiceUrl;
            this.conversationReference.Conversation = this.conversationAccount;
            this.currentUserId = turnContext.Activity.From.Id;
            await ((BotFrameworkAdapter)this.adapter).ContinueConversationAsync(this.configuration["MicrosoftAppId"], this.conversationReference, this.BotCallback, default(CancellationToken));

            return this.isUserPartOfRoster;
        }

        // BotCallback which will get members of the team and check if the SME is a already part of
        // roster or not
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var members = await ((BotFrameworkAdapter)this.adapter).GetConversationMembersAsync(turnContext, default(CancellationToken));

                foreach (var member in members)
                {
                    if (member.Id.Equals(this.currentUserId))
                    {
                        this.isUserPartOfRoster = true;
                    }
                }
            }
            catch (Exception error)
            {
                this.telemetryClient.TrackTrace($"Bot is not installed into Teams {turnContext.Activity.Name}: {error.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(error);
                this.isUserPartOfRoster = false;
            }
        }
    }
}
