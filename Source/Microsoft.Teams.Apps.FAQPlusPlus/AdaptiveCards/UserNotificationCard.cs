// <copyright file="UserNotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Creates a user notification card from a ticket.
    /// </summary>
    public class UserNotificationCard
    {
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
        /// <param name="message">The status message to add to the card</param>
        /// <returns>An adaptive card as an attachment</returns>
        public Attachment ToAttachment(string message)
        {
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
                                Title = "Created:",
                                Value = "{{DATE(" + this.ticket.DateCreated.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ", SHORT)}} {{TIME(" + this.ticket.DateCreated.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ")}}",
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = this.GetTicketClosedDate(this.ticket),
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

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A status string.</returns>
        private string GetTicketStatus(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Open)
            {
                return string.IsNullOrEmpty(ticket.AssignedToName) ? "Open" : "Assigned";
            }
            else
            {
                return "Closed";
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