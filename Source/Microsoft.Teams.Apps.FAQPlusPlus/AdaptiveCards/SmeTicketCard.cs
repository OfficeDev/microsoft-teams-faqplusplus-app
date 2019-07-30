// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;

    public class SmeTicketCard
    {
        private TicketEntity ticketModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public SmeTicketCard(TicketEntity ticket)
        {
            this.ticketModel = ticket;
        }

        /// <summary>
        /// Method to generate the adaptive card.
        /// </summary>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticketModel.OpenedBy != null ?
                         string.Format("**{0}** is requesting support. Details as follows:", this.ticketModel.OpenedBy) :
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
                                Value = this.ticketModel.UserTitleText,
                            },
                            new AdaptiveFact
                            {
                                Title = "Description:",
                                Value = this.ticketModel.Text,
                            },
                            new AdaptiveFact
                            {
                                Title = "Knowledge Base Entry:",
                                Value = this.ticketModel.KbEntryResponse != null ? this.ticketModel.KbEntryResponse : "N/A",
                            },
                            new AdaptiveFact
                            {
                                Title = "Question asked:",
                                Value = this.ticketModel.KbEntryQuestion != null ? this.ticketModel.KbEntryQuestion : "N/A",
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
                        Title = $"Chat with {this.ticketModel.OpenedByFirstName}",
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={this.ticketModel.OpenedByUpn}"),
                    },
                    new AdaptiveShowCardAction
                    {
                        Title = "Status",
                        Card = new AdaptiveCard
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveChoiceSetInput
                                {
                                    Id = "statuscode",
                                    IsMultiSelect = false,
                                    Style = AdaptiveChoiceInputStyle.Compact,
                                    Value = this.ticketModel.Status.ToString(),
                                    Choices = new List<AdaptiveChoice>
                                    {
                                        new AdaptiveChoice
                                        {
                                           Title = "Open",
                                           Value = "1",
                                        },
                                        new AdaptiveChoice
                                        {
                                            Title = "Assign",
                                            Value = "2",
                                        },
                                        new AdaptiveChoice
                                        {
                                            Title = "Closed",
                                            Value = "0",
                                        },
                                    },
                                },
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Type = "Action.Submit",
                                    DataJson = JsonConvert.SerializeObject(new { rowKey = this.ticketModel.TicketId })
                                }
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>A status string.</returns>
        private string GetTicketStatus(TicketEntity ticketModel)
        {
            if (ticketModel.Status == 1 && string.IsNullOrEmpty(ticketModel.AssignedTo))
            {
                return "Open";
            }
            else if (ticketModel.Status == 1 && !string.IsNullOrEmpty(ticketModel.AssignedTo))
            {
                return $"Assigned to {ticketModel.AssignedTo}";
            }
            else
            {
                return $"Closed by {ticketModel.AssignedTo}";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticketModel)
        {
            return ticketModel.Status == 0 ? DateTime.Now.ToString("D") : "N/A";
        }
    }
}