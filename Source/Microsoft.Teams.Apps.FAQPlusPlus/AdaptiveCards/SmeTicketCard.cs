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
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an SME ticket used for both in place card update activity within Sme channel
    /// when changing the ticket status and notification card when bot posts user question to Sme channel.
    /// </summary>
    public class SmeTicketCard
    {
        private const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
        private const string Ellipsis = "...";
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
        /// <param name="questionForExpert">When user activity is question for expert.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(string questionForExpert = null)
        {
            var ticketCreatedDate = this.ticket.DateCreated.ToString(DateFormat);
            var title = new AdaptiveTextBlock
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.AskAnExpertText1,
                Color = AdaptiveTextColor.Attention,
                Size = AdaptiveTextSize.Medium
            };
            if (this.ticket.KnowledgeBaseAnswer != null && this.ticket.KnowledgeBaseAnswer.Length > 500)
            {
                this.ticket.KnowledgeBaseAnswer = this.ticket.KnowledgeBaseAnswer.Substring(0, 500) + Ellipsis;
            }

            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    questionForExpert == SubmitUserRequestPayload.QuestionForExpertAction ? title : new AdaptiveTextBlock(),
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
                                Value = CardHelper.GetTicketStatus(this.ticket),
                            },
                            new AdaptiveFact
                            {
                                Title = "Title:",
                                Value = this.ticket.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = "Description:",
                                Value = CardHelper.GetDescriptionText(this.ticket.Description),
                            },
                            new AdaptiveFact
                            {
                                Title = "Knowledge Base Entry:",
                                Value = this.GetKbAnswer(),
                            },
                            new AdaptiveFact
                            {
                                Title = "Question asked:",
                                Value = this.GetUserQuestion(),
                            },
                            new AdaptiveFact
                            {
                                Title = "Created:",
                                Value = "{{DATE(" + ticketCreatedDate + ", SHORT)}} {{TIME(" + ticketCreatedDate + ")}}"
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = CardHelper.GetTicketClosedDate(this.ticket),
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
                        }
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        private string GetKbAnswer()
        {
            return !string.IsNullOrEmpty(this.ticket.KnowledgeBaseAnswer) ? this.ticket.KnowledgeBaseAnswer : Resource.NonApplicableString;
        }

        private string GetUserQuestion()
        {
            return !string.IsNullOrEmpty(this.ticket.UserQuestion) ? this.ticket.UserQuestion : Resource.NonApplicableString;
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
    }
}