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
    using Newtonsoft.Json;

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
        /// <param name="localTimeStamp">Local time stamp of the user activity.</param>
        /// <param name="isQuestionForExpert"> Pass true when user activity is question for expert.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimeStamp, bool isQuestionForExpert = false)
        {
            var ticketCreatedDate = CardHelper.GetLocalTimeStamp(localTimeStamp);
            var questionForExpertTitle = new AdaptiveTextBlock();
            if (isQuestionForExpert)
            {
                questionForExpertTitle.Weight = AdaptiveTextWeight.Bolder;
                questionForExpertTitle.Text = Resource.AskAnExpertText1;
                questionForExpertTitle.Color = AdaptiveTextColor.Attention;
            }

            var kbAnswer = CardHelper.TruncateStringIfLonger(this.ticket.KnowledgeBaseAnswer, CardHelper.KbAnswerMaxLength);

            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    questionForExpertTitle,
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
                                Title = Resource.TitleText,
                                Value = this.ticket.Title,
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DescriptionText,
                                Value = CardHelper.GetDescriptionText(this.ticket.Description),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.KBEntryFactTitle,
                                Value = this.GetKbAnswer(kbAnswer),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.QuestionAskedFactTitle,
                                Value = this.GetUserQuestion(),
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.DateCreatedDisplayFactTitle,

                                // We are using this format because DATE and TIME are not supported on mobile yet.
                                Value = ticketCreatedDate
                            },
                            new AdaptiveFact
                            {
                                Title = Resource.ClosedFactTitle,
                                Value = CardHelper.GetTicketClosedDate(this.ticket, localTimeStamp),
                            }
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = string.Format(Resource.ChatTextButton, this.ticket.RequesterGivenName),
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={this.ticket.RequesterUserPrincipalName}"),
                    },
                    new AdaptiveShowCardAction
                    {
                        Title = Resource.ChangeStatusButtonText,
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

        private string GetKbAnswer(string kbAnswer)
        {
            return !string.IsNullOrEmpty(kbAnswer) ? kbAnswer : Resource.NonApplicableString;
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
                            Title = Resource.AssignStatusTitle,
                            Value = ChangeTicketStatusPayload.AssignToSelfAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = Resource.ClosedFactTitle,
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
                            Title = Resource.OpenStatusTitle,
                            Value = ChangeTicketStatusPayload.ReopenAction,
                        },
                        new AdaptiveChoice
                        {
                            Title = Resource.AssignStatusTitle,
                            Value = ChangeTicketStatusPayload.AssignToSelfAction,
                        },
                    },
                };
            }

            return adaptiveChoices;
        }
    }
}