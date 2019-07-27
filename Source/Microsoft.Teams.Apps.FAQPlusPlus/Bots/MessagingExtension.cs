namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;

    public class MessagingExtension
    {
        private const int TextTrimLengthForCard = 10;

        private readonly ISearchService searchService;

        private readonly ITicketsProvider ticket;

        public MessagingExtension(ISearchService searchService, ITicketsProvider ticket)
        {
            this.searchService = searchService;
            this.ticket = ticket;
        }

        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext)
        {

            TicketEntity ticket = new TicketEntity()
            {
                AssignedTo = "Anurag",
                CardActivityId = "f:5392518584933790257",
                DateAssigned = System.DateTime.Now,
                DateCreated = System.DateTime.Now,
                OpenedBy = "Anurag1",
                OpenedByConversationId = "11111",
                Status = 1,
                Text = "Comments",
                ThreadConversationId = "11aa1",
                TicketId = System.Guid.NewGuid().ToString()
            };

            await this.ticket.SaveOrUpdateTicketEntityAsync(ticket);
            // delete above line

            try
            {
                if (turnContext.Activity.Name == "composeExtension/query")
                {
                    var messageExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
                    var searchQuery = this.GetMessagingExtensionQueryParameter(messageExtensionQuery);

                    return new InvokeResponse
                    {
                        Body = new MessagingExtensionResponse
                        {
                            ComposeExtension = await this.GetSearchResultAsync(searchQuery, messageExtensionQuery.CommandId),
                        },
                        Status = 200,
                    };
                }
                else
                {
                    return this.MessageExtensionErrorResponse("Activity not of type query");
                }
            }
            catch (Exception error)
            {
                return this.MessageExtensionErrorResponse(error.Message);
            }
        }

        public async Task<MessagingExtensionResult> GetSearchResultAsync(string query, string commandId)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            List<TicketEntity> searchServiceResults = null;

            // commandId should be equal to Id mentioned in Manifet file under composeExtensions section
            switch (commandId)
            {
                case "recents":
                    searchServiceResults = await this.searchService.SMESearchServiceForMessageExtension(query, MessagingExtensionConstants.RecentTabType);
                    break;

                case "openrequests":
                    searchServiceResults = await this.searchService.SMESearchServiceForMessageExtension(query, MessagingExtensionConstants.OpenTabType);
                    break;

                case "assignedrequests":
                    searchServiceResults = await this.searchService.SMESearchServiceForMessageExtension(query, MessagingExtensionConstants.AssignedTabType);
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

        // True and false to identify if the request is for preview or card
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

        private InvokeResponse MessageExtensionErrorResponse(string message)
        {
            return new InvokeResponse
            {
                Body = new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Text = "Failed to search due to " + message,
                        Type = "message"
                    },
                },
                Status = 200,
            };
        }

        private string GetMessagingExtensionQueryParameter(MessagingExtensionQuery query)
        {
            string messageExtensionInputText = string.Empty;
            foreach (var response in query.Parameters)
            {
                if (response.Name != "initialRun")
                {
                    messageExtensionInputText = response.Value.ToString();
                }
            }

            return messageExtensionInputText;
        }
    }
}
