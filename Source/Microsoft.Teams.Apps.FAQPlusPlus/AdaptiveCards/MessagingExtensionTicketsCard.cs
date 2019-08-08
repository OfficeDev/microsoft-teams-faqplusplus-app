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
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Implements messaging extension tickets card.
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
                    new AdaptiveFactSet
                    {
                        Facts = this.GetAdaptiveFactSetForTitle(this.ticketModel),
                    },
                    new AdaptiveTextBlock
                    {
                        Text = this.ticketModel.RequesterName != null ?
                         string.Format(Resource.QuestionForExpertSubHeaderText, this.ticketModel.RequesterName) : string.Empty,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = this.GetAdaptiveFactSet(this.ticketModel),
                    },
                },
                Actions = this.GetAdaptiveAction(this.ticketModel),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Create adaptivefactset for the title
        private List<AdaptiveFact> GetAdaptiveFactSetForTitle(TicketEntity ticketModel)
        {
            List<AdaptiveFact> adaptivefact = new List<AdaptiveFact>();
            if (!string.IsNullOrEmpty(ticketModel.Title) && !ticketModel.Title.Equals(Resource.NonApplicableString))
            {
                adaptivefact.Add(new AdaptiveFact { Title = Resource.TitleText, Value = ticketModel.Title });
            }

            return adaptivefact;
        }

        // Create adaptivefact set which can be displayed in the card below requestor
        private List<AdaptiveFact> GetAdaptiveFactSet(TicketEntity ticketModel)
        {
            List<AdaptiveFact> adaptivefact = new List<AdaptiveFact>();
            if (!string.IsNullOrEmpty(ticketModel.Description) && !ticketModel.Description.Equals(Resource.NonApplicableString))
            {
                adaptivefact.Add(new AdaptiveFact { Title = Resource.DescriptionText, Value = ticketModel.Description });
            }

            adaptivefact.Add(new AdaptiveFact { Title = Resource.StatusFactTitle, Value = this.GetTicketStatus(this.ticketModel) });

            if (!string.IsNullOrEmpty(ticketModel.UserQuestion) && !ticketModel.UserQuestion.Equals(Resource.NonApplicableString))
            {
                adaptivefact.Add(new AdaptiveFact { Title = Resource.QuestionText, Value = ticketModel.UserQuestion });
            }

            if (ticketModel.DateClosed != null)
            {
                string closedDate = this.GetTicketClosedDate(this.ticketModel);
                if (!string.IsNullOrEmpty(closedDate))
                {
                    adaptivefact.Add(new AdaptiveFact { Title = Resource.ClosedFactTitle, Value = closedDate });
                }
            }

            return adaptivefact;
        }

        // Create adaptiveaction buttons for invoking chat and go to thread button action
        private List<AdaptiveAction> GetAdaptiveAction(TicketEntity ticketModel)
        {
            List<AdaptiveAction> adaptiveAction = new List<AdaptiveAction>();
            if (!string.IsNullOrEmpty(ticketModel.RequesterGivenName))
            {
                adaptiveAction.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Type = "Action.OpenUrl",
                        Title = $"{string.Format(Resource.ChatTextButton, ticketModel.RequesterGivenName)}",
                        Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={ticketModel.RequesterUserPrincipalName}"),
                    });
            }

            if (!string.IsNullOrEmpty(ticketModel.SmeThreadConversationId))
            {
                adaptiveAction.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Type = "Action.OpenUrl",
                        Title = $"{Resource.GoToOriginalThreadButtonText}",
                        Url = new Uri(this.GetGoToThreadUri(ticketModel.SmeThreadConversationId))
                    });
            }

            return adaptiveAction;
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
                return $"{Resource.UnassignedStatusText}";
            }
            else if (ticketModel.Status == (int)TicketState.Open && !string.IsNullOrEmpty(ticketModel.AssignedToName))
            {
                return $"{string.Format(Resource.AssignedToStatusValue, ticketModel.AssignedToName)}";
            }
            else
            {
                return $"{Resource.CloseStatusText}";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticketModel)
        {
            return ticketModel.Status == (int)TicketState.Closed ? ticketModel.DateClosed.Value.ToString("D") : string.Empty;
        }
    }
}
