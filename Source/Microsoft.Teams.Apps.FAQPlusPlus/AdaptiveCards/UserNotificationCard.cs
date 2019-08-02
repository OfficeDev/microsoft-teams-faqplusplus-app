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

    public class UserNotificationCard
    {
        private TicketEntity ticketModel;

        public UserNotificationCard(TicketEntity ticketModel)
        {
            this.ticketModel = ticketModel;
        }

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
                                Title = "Created:",
                                Value = "{{DATE(" + this.ticketModel.DateCreated.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ", SHORT)}} {{TIME(" + this.ticketModel.DateCreated.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ")}}",
                            },
                            new AdaptiveFact
                            {
                                Title = "Closed:",
                                Value = this.GetTicketClosedDate(this.ticketModel),
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
                return "Assigned";
            }
            else
            {
                return "Closed";
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticketModel">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        private string GetTicketClosedDate(TicketEntity ticketModel)
        {
            return ticketModel.Status == 1 ? "{{DATE(" + ticketModel.DateClosed?.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ", SHORT)}} {{TIME(" + ticketModel.DateClosed?.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ") + ")}}" : Resource.NotApplicable;
        }
    }
}