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
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <returns>Returns the attachment that will be attached to messaging extension list.</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimestamp)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveFactSet
                    {
                        Facts = this.GetAdaptiveFactsSetForTitle(this.ticketModel),
                    },
                    new AdaptiveTextBlock
                    {
                        Text = string.Format(Resource.QuestionForExpertSubHeaderText, this.ticketModel.RequesterName),
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = this.GetAdaptiveFactsSet(this.ticketModel, localTimestamp),
                    },
                },
                Actions = this.GetAdaptiveActions(this.ticketModel),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Create adaptivefactsset for the title
        private List<AdaptiveFact> GetAdaptiveFactsSetForTitle(TicketEntity ticketModel)
        {
            List<AdaptiveFact> adaptivefacts = new List<AdaptiveFact>();
            adaptivefacts.Add(new AdaptiveFact { Title = Resource.TitleText, Value = ticketModel.Title });

            return adaptivefacts;
        }

        // Create adaptivefacts set which can be displayed in the card below requestor
        private List<AdaptiveFact> GetAdaptiveFactsSet(TicketEntity ticketModel, DateTimeOffset? localTimestamp)
        {
            List<AdaptiveFact> adaptivefacts = new List<AdaptiveFact>();
            if (!string.IsNullOrEmpty(ticketModel.Description))
            {
                adaptivefacts.Add(new AdaptiveFact { Title = Resource.DescriptionText, Value = ticketModel.Description });
            }

            adaptivefacts.Add(new AdaptiveFact { Title = Resource.StatusFactTitle, Value = CardHelper.GetTicketDisplayStatusForSme(this.ticketModel) });

            if (!string.IsNullOrEmpty(ticketModel.UserQuestion))
            {
                adaptivefacts.Add(new AdaptiveFact { Title = Resource.QuestionAskedFactTitle, Value = ticketModel.UserQuestion });
            }

            if (ticketModel.DateClosed != null)
            {
                string closedDate = CardHelper.GetFormattedDateInUserTimeZone(this.ticketModel.DateClosed.Value, localTimestamp);
                adaptivefacts.Add(new AdaptiveFact { Title = Resource.ClosedFactTitle, Value = closedDate });
            }

            return adaptivefacts;
        }

        // Create adaptiveactions buttons for invoking chat and go to thread button action
        private List<AdaptiveAction> GetAdaptiveActions(TicketEntity ticketModel)
        {
            List<AdaptiveAction> adaptiveActions = new List<AdaptiveAction>();
            adaptiveActions.Add(
                new AdaptiveOpenUrlAction
                {
                    Title = $"{string.Format(Resource.ChatTextButton, ticketModel.RequesterGivenName)}",
                    Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(ticketModel.RequesterUserPrincipalName)}"),
                });

            if (!string.IsNullOrEmpty(ticketModel.SmeThreadConversationId))
            {
                adaptiveActions.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Title = $"{Resource.GoToOriginalThreadButtonText}",
                        Url = new Uri(this.GetGoToThreadUri(ticketModel.SmeThreadConversationId))
                    });
            }

            return adaptiveActions;
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
    }
}
