// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Represents an SME ticket used for both in place card update activity within SME channel
    /// when changing the ticket status and notification card when bot posts user question to SME channel.
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
        /// <param name="activityLocalTimestamp">Local time stamp of the user activity.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(DateTimeOffset? activityLocalTimestamp)
        {
            var kbAnswer = CardHelper.TruncateStringIfLonger(this.ticket.KnowledgeBaseAnswer, CardHelper.KbAnswerMaxLength);

            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticket.RequesterName != null ?
                            string.Format(Resource.QuestionForExpertSubHeaderText, this.ticket.RequesterName) :
                            Resource.SmeAttentionText,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = new List<AdaptiveFact>
                        {
                            new AdaptiveFact
                            {
                                Title = Resource.StatusFactTitle,
                                Value = CardHelper.GetTicketStatus(this.ticket),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.TitleFact,
                                Value = this.ticket.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DescriptionText,
                                Value = CardHelper.ConvertNullOrEmptyToNotApplicable(this.ticket.Description),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.KBEntryFactTitle,
                                Value = CardHelper.ConvertNullOrEmptyToNotApplicable(kbAnswer),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.QuestionAskedFactTitle,
                                Value = CardHelper.ConvertNullOrEmptyToNotApplicable(this.ticket.UserQuestion),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DateCreatedDisplayFactTitle,
                                Value = CardHelper.GetFormattedDateInUserTimeZone(this.ticket.DateCreated, activityLocalTimestamp),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.ClosedFactTitle,
                                Value = CardHelper.GetTicketClosedDate(this.ticket, activityLocalTimestamp),
                            }
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = string.Format(Resource.ChatTextButton, this.ticket.RequesterGivenName),
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(this.ticket.RequesterUserPrincipalName)}"),
                    },
                    new AdaptiveShowCardAction
                    {
                        Title = Resource.ChangeStatusButtonText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                this.GetAdaptiveChoiceSetInput(this.ticket),
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Data = new ChangeTicketStatusPayload { TicketId = this.ticket.TicketId }
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

        private AdaptiveElement GetAdaptiveChoiceSetInput(TicketEntity ticket)
        {
            AdaptiveChoiceSetInput choiceSet = new AdaptiveChoiceSetInput
            {
                Id = nameof(ChangeTicketStatusPayload.Action),
                IsMultiSelect = false,
                Style = AdaptiveChoiceInputStyle.Compact
            };

            if (ticket.Status == (int)TicketState.Open && string.IsNullOrEmpty(ticket.AssignedToName))
            {
                choiceSet.Value = ChangeTicketStatusPayload.AssignToSelfAction;
                choiceSet.Choices = new List<AdaptiveChoice>
                {
                    new AdaptiveChoice
                    {
                        Title = Resource.AssignStatusTitle,
                        Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.ClosedFactTitle,
                        Value = ChangeTicketStatusPayload.CloseAction,
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Open && !string.IsNullOrEmpty(ticket.AssignedToName))
            {
                choiceSet.Value = ChangeTicketStatusPayload.CloseAction;
                choiceSet.Choices = new List<AdaptiveChoice>
                {
                    new AdaptiveChoice
                    {
                        Title = Resource.OpenStatusTitle,
                        Value = ChangeTicketStatusPayload.ReopenAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.AssignStatusTitle,
                        Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.ClosedFactTitle,
                        Value = ChangeTicketStatusPayload.CloseAction,
                    },
                };
            }
            else if (ticket.Status == (int)TicketState.Closed)
            {
                choiceSet.Value = ChangeTicketStatusPayload.ReopenAction;
                choiceSet.Choices = new List<AdaptiveChoice>
                {
                    new AdaptiveChoice
                    {
                        Title = Resource.OpenStatusTitle,
                        Value = ChangeTicketStatusPayload.ReopenAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.AssignStatusTitle,
                        Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    },
                };
            }

            return choiceSet;
        }
    }
}