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
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimestamp)
        {
            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.ticket.Title,
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = this.ticket.Description != null ?
                            string.Format(Resource.QuestionForExpertSubHeaderText, this.ticket.RequesterName, this.ticket.Description) :
                            Resource.SmeAttentionText,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = this.BuildFactSet(this.ticket, localTimestamp),
                    },
                },
                Actions = this.BuildActions(this.ticket),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Builds out the fact set for the SME Ticket card.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <param name="localTimestamp">The current timestamp.</param>
        /// <returns>The fact set showing the necessary details.</returns>
        private List<AdaptiveFact> BuildFactSet(TicketEntity ticket, DateTimeOffset? localTimestamp)
        {
            List<AdaptiveFact> factSetList = new List<AdaptiveFact>();

            factSetList.Add(new AdaptiveFact
            {
                Title = Resource.StatusFactTitle,
                Value = CardHelper.GetTicketDisplayStatusForSme(this.ticket),
            });

            if (!string.IsNullOrEmpty(ticket.UserQuestion))
            {
                factSetList.Add(new AdaptiveFact
                {
                    Title = Resource.QuestionAskedFactTitle,
                    Value = CardHelper.TruncateStringIfLonger(ticket.Description, CardHelper.UserDescriptionMaxLength)
                });
            }

            if (ticket.Status == (int)TicketState.Closed)
            {
                factSetList.Add(new AdaptiveFact
                {
                    Title = Resource.ClosedFactTitle,
                    Value = CardHelper.GetTicketClosedDate(this.ticket, localTimestamp),
                });
            }

            return factSetList;
        }

        /// <summary>
        /// Making sure to build out the adaptive card actions.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>Adaptive card actions.</returns>
        private List<AdaptiveAction> BuildActions(TicketEntity ticket)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();

            var messageToSend = string.Format(Resource.SmeUserChatMessage, ticket.Title);
            var encodedMessage = Uri.EscapeDataString(messageToSend);

            actionsList.Add(new AdaptiveOpenUrlAction
            {
                Title = string.Format(Resource.ChatTextButton, this.ticket.RequesterGivenName),
                Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(this.ticket.RequesterUserPrincipalName)}&message={encodedMessage}")
            });

            actionsList.Add(new AdaptiveShowCardAction
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
            });

            if (!string.IsNullOrEmpty(ticket.KnowledgeBaseAnswer))
            {
                actionsList.Add(new AdaptiveShowCardAction
                {
                    Title = Resource.ViewArticleButtonText,
                    Card = new AdaptiveCard("1.0")
                    {
                        Body = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = ticket.KnowledgeBaseAnswer,
                                    Wrap = true,
                                }
                            },
                    },
                });
            }

            return actionsList;
        }

        /// <summary>
        /// Method to get the dropdown and the correct values to render.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>An adaptive element which contains the dropdown choices.</returns>
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
                        Title = Resource.CloseStatusTitle,
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
                        Title = Resource.UnassignStatusTitle,
                        Value = ChangeTicketStatusPayload.ReopenAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.AssignStatusTitle,
                        Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.CloseStatusTitle,
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
                        Title = Resource.UnassignStatusTitle,
                        Value = ChangeTicketStatusPayload.ReopenAction,
                    },
                    new AdaptiveChoice
                    {
                        Title = Resource.ReassignStatusTitle,
                        Value = ChangeTicketStatusPayload.AssignToSelfAction,
                    },
                };
            }

            return choiceSet;
        }
    }
}