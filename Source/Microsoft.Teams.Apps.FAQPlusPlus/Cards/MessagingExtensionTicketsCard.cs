// <copyright file="MessagingExtensionTicketsCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Implements messaging extension.
    /// </summary>
    public class MessagingExtensionTicketsCard
    {
        private readonly TicketEntity ticketModel;

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
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticketModel.RequesterName != null ?
                         string.Format(Resource.QuestionForExpertSubHeaderText, this.ticketModel.RequesterName) :
                        Resource.SmeAttentionText,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = new List<AdaptiveFact>
                        {
                            new AdaptiveFact
                            {
                                Title = Resource.OpenStatusTitle,
                                Value = this.GetTicketStatus(this.ticketModel),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.TitleText,
                                Value = this.ticketModel.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DescriptionText,
                                Value = this.ticketModel.Description,
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.KBEntryFactTitle,
                                Value = this.ticketModel.KnowledgeBaseAnswer != null ? this.ticketModel.KnowledgeBaseAnswer : "N/A",
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.QuestionAskedFactTitle,
                                Value = this.ticketModel.UserQuestion != null ? this.ticketModel.UserQuestion : "N/A",
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DateCreatedDisplayFactTitle,
                                Value = this.ticketModel.DateCreated.ToString("ddd, MMM dd',' yyy hh':'mm tt"),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DateCreatedDisplayFactTitle,
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
