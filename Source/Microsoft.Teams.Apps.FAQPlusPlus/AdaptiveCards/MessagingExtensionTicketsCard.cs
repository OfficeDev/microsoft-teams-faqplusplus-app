// <copyright file="MessagingExtensionTicketsCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    public class MessagingExtensionTicketsCard
    {
        private TicketEntity ticketModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionTicketsCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public MessagingExtensionTicketsCard(TicketEntity ticket)
        {
            this.ticketModel = ticket;
        }

        /// <summary>
        /// Method to generate the adaptive card.
        /// </summary>
        /// <returns>Returns the attachment that will be attached to messaging extension list.</returns>
        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticketModel.RequesterName != null ?
                         string.Format("**{0}** is requesting support. Details as follows:", this.ticketModel.RequesterName) :
                        "Everyone there is a new request coming in, please see the details below:",
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = new List<AdaptiveFact>
                        {
                            new AdaptiveFact
                            {
                                Title = "Status:",
                                Value = this.GetTicketStatus(this.ticketModel),
                            },
                            new AdaptiveFact
                            {
                                Title = "Title:",
                                Value = this.ticketModel.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = "Description:",
                                Value = this.ticketModel.Description,
                            },
                            new AdaptiveFact
                            {
                                Title = "Knowledge Base Entry:",
                                Value = this.ticketModel.KnowledgeBaseAnswer != null ? this.ticketModel.KnowledgeBaseAnswer : "N/A",
                            },
                            new AdaptiveFact
                            {
                                Title = "Question asked:",
                                Value = this.ticketModel.UserQuestion != null ? this.ticketModel.UserQuestion : "N/A",
                            },
                            new AdaptiveFact
                            {
                                Title = "Created:",
                                Value = this.ticketModel.DateCreated.ToString("D"),
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = this.GetTicketClosedDate(this.ticketModel),
                            }
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Type = "Action.OpenUrl",
                        Title = $"Chat with {this.ticketModel.RequesterGivenName}",
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={this.ticketModel.RequesterUserPrincipalName}"),
                    },
                    new AdaptiveOpenUrlAction
                    {
                        Type = "Action.OpenUrl",
                        Title = $"Go to original thread",
                        Url = new Uri(this.GetGoToThreadUri(this.ticketModel.SmeThreadConversationId))
                    }
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Returns go to original thread uri which will help in opening the original conversation about the ticket
        /// </summary>
        /// <param name="threadConversationId">The thread along with message Id stored in storage table.</param>
        /// <returns>original thread uri.</returns>
        private string GetGoToThreadUri(string threadConversationId)
        {
            string returnUri = $"https://teams.microsoft.com/l/message/";
            if (!string.IsNullOrEmpty(threadConversationId) && threadConversationId.Contains(";"))
            {
                string[] threadAndMessageId = threadConversationId.Split(";");
                return returnUri + $"{threadAndMessageId[0]}/{threadAndMessageId[1].Split("=")[1]}";
            }

            return returnUri;
        }

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>A status string.</returns>
        private string GetTicketStatus(TicketEntity ticketModel)
        {
            if (ticketModel.Status == (int)TicketState.Open && string.IsNullOrEmpty(ticketModel.AssignedToName))
            {
                return "Open";
            }
            else if (ticketModel.Status == (int)TicketState.Open && !string.IsNullOrEmpty(ticketModel.AssignedToName))
            {
                return $"Assigned to {ticketModel.AssignedToName}";
            }
            else
            {
                return $"Closed by {ticketModel.LastModifiedByName}";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticketModel)
        {
            return ticketModel.Status == (int)TicketState.Closed ? ticketModel.DateClosed.Value.ToString("D") : "N/A";
        }
    }
}
