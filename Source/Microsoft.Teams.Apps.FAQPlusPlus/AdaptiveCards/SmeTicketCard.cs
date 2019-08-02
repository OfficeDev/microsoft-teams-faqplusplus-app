﻿// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an SME ticket.
    /// </summary>
    public class SmeTicketCard
    {
        private readonly TicketEntity ticket;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public SmeTicketCard(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Method to generate the adaptive card.
        /// </summary>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticket.RequesterName != null ?
                         string.Format("**{0}** is requesting support. Details as follows:", this.ticket.RequesterName) :
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
                                Value = this.GetTicketStatus(this.ticket),
                            },
                            new AdaptiveFact
                            {
                                Title = "Title:",
                                Value = this.ticket.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = "Description:",
                                Value = this.ticket.Description,
                            },
                            new AdaptiveFact
                            {
                                Title = "Knowledge Base Entry:",
                                Value = this.ticket.KnowledgeBaseAnswer,
                            },
                            new AdaptiveFact
                            {
                                Title = "Question asked:",
                                Value = this.ticket.UserQuestion,
                            },
                            new AdaptiveFact
                            {
                                Title = "Created:",
                                Value = this.ticket.DateCreated.ToString("D"),
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = this.GetTicketClosedDate(this.ticket),
                            }
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = $"Chat with {this.ticket.RequesterGivenName}",
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={this.ticket.RequesterUserPrincipalName}"),
                    },
                    new AdaptiveShowCardAction
                    {
                        Title = "Change status",
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                this.GetAdaptiveInputSet(this.ticket),
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    DataJson = JsonConvert.SerializeObject(new ChangeTicketStatusPayload { TicketId = this.ticket.TicketId })
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
                    Id = "action",
                    IsMultiSelect = false,
                    Style = AdaptiveChoiceInputStyle.Compact,
                    Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    Choices = new List<AdaptiveChoice>
                    {
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = ChangeTicketStatusPayload.AssignToSelfAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = "Closed",
                            Value = ChangeTicketStatusPayload.CloseAction,
                        },
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Open && !string.IsNullOrEmpty(ticket.AssignedToName))
            {
                adaptiveChoices = new AdaptiveChoiceSetInput
                {
                    Id = "action",
                    IsMultiSelect = false,
                    Style = AdaptiveChoiceInputStyle.Compact,
                    Value = ChangeTicketStatusPayload.CloseAction,
                    Choices = new List<AdaptiveChoice>
                    {
                        new AdaptiveChoice
                        {
                            Title = "Open",
                            Value = ChangeTicketStatusPayload.ReopenAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = ChangeTicketStatusPayload.AssignToSelfAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = "Closed",
                            Value = ChangeTicketStatusPayload.CloseAction,
                        },
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Closed)
            {
                adaptiveChoices = new AdaptiveChoiceSetInput
                {
                    Id = "action",
                    IsMultiSelect = false,
                    Style = AdaptiveChoiceInputStyle.Compact,
                    Value = ChangeTicketStatusPayload.ReopenAction,
                    Choices = new List<AdaptiveChoice>
                    {
                        new AdaptiveChoice
                        {
                            Title = "Open",
                            Value = ChangeTicketStatusPayload.ReopenAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = "Assign",
                            Value = ChangeTicketStatusPayload.AssignToSelfAction,
                        },
                    },
                };
            }

            return adaptiveChoices;
        }

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A status string.</returns>
        private string GetTicketStatus(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Open)
            {
                return string.IsNullOrEmpty(ticket.AssignedToName) ? "Open" : $"Assigned to {ticket.AssignedToName}";
            }
            else
            {
                return $"Closed by {ticket.LastModifiedByName}";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Closed)
            {
                var dateClosed = ticket.DateClosed.Value;
                return "{{DATE(" + dateClosed.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ", SHORT)}} {{TIME(" + dateClosed.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ")}}";
            }
            else
            {
                return Resource.NotApplicable;
            }
        }
    }
}