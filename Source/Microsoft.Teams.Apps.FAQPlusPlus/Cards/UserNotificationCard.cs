// <copyright file="UserNotificationCard.cs" company="Microsoft">
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
    /// Creates a user notification card from a ticket.
    /// </summary>
    public class UserNotificationCard
    {
        private const string DateFormat = "ddd, MMM dd',' yyy hh':'mm tt";

        private readonly TicketEntity ticket;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket to create a card from</param>
        public UserNotificationCard(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Returns a user notification card for the ticket.
        /// </summary>
        /// <param name="localTimeStamp">Local time stamp of user activity.</param>
        /// <param name="message">The status message to add to the card</param>
        /// <returns>An adaptive card as an attachment</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimeStamp, string message)
        {
            var ticketCreatedDate = CardHelper.GetLocalTimeStamp(localTimeStamp);
            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.NotificationCardTitleText,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = message,
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
                                Title = "Created:",

                                // We are using this format because DATE and TIME are not supported on mobile yet.
                                Value = ticketCreatedDate,
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = CardHelper.GetTicketClosedDate(this.ticket, localTimeStamp),
                            }
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
    }
}