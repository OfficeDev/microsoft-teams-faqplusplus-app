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
                                Value = this.ticketModel.KnowledgeBaseAnswer,
                            },
                            new AdaptiveFact
                            {
                                Title = "Question asked:",
                                Value = this.ticketModel.UserQuestion,
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
                    new AdaptiveShowCardAction
                    {
                        Title = "Change status",
                        Card = new AdaptiveCard
                        {
                            Body = new List<AdaptiveElement>
                            {
                                this.GetAdaptiveInputSet(this.ticketModel),
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

        private AdaptiveElement GetAdaptiveInputSet(TicketEntity ticket)
        {
            AdaptiveChoiceSetInput adaptiveChoices = null;
            if (ticket.Status == (int)TicketState.Open && string.IsNullOrEmpty(ticket.AssignedToName))
            {
                adaptiveChoices = new AdaptiveChoiceSetInput
                {
                    Id = "statuscode",
                    IsMultiSelect = false,
                    Style = AdaptiveChoiceInputStyle.Compact,
                    Value = this.ticketModel.Status.ToString(),
                    Choices = new List<AdaptiveChoice>
                    {
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = "2",
                        },
                        new AdaptiveChoice
                        {
                            Title = "Closed",
                            Value = "1",
                        },
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Open && !string.IsNullOrEmpty(ticket.AssignedToName))
            {
                adaptiveChoices = new AdaptiveChoiceSetInput
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
                            Value = "0",
                        },
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = "2",
                        },
                        new AdaptiveChoice
                        {
                            Title = "Closed",
                            Value = "1",
                        },
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Closed)
            {
                adaptiveChoices = new AdaptiveChoiceSetInput
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
                            Value = "0",
                        },
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = "2",
                        },
                    },
                };
            }

            return adaptiveChoices;
        }

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>A status string.</returns>
        private string GetTicketStatus(TicketEntity ticketModel)
        {
            if (ticketModel.Status == 0 && string.IsNullOrEmpty(ticketModel.AssignedToName))
            {
                return "Open";
            }
            else if (ticketModel.Status == 0 && !string.IsNullOrEmpty(ticketModel.AssignedToName))
            {
                return $"Assigned to {ticketModel.AssignedToName}";
            }
            else
            {
                return $"Closed by {ticketModel.AssignedToName}";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticketModel)
        {
            return ticketModel.Status == 1 ? DateTime.UtcNow.ToString("D") : "N/A";
        }
    }
}