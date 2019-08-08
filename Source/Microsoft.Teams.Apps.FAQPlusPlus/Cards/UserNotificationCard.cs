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
        /// <param name="activityLocalTimestamp">Local time stamp of user activity.</param>
        /// <returns>An adaptive card as an attachment</returns>
        public Attachment ToAttachment(string message, DateTimeOffset? activityLocalTimestamp)
        {
            var card = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = message,
                        Wrap = true,
                    },
                    this.BuildFactSet(this.ticket, activityLocalTimestamp),
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        private AdaptiveElement BuildFactSet(TicketEntity ticket, DateTimeOffset? activityLocalTimestamp)
        {
            if (ticket.Status == (int)TicketState.Open)
            {
                return new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact
                        {
                            Title = Resource.StatusFactTitle,
                            Value = CardHelper.GetUserTicketStatus(this.ticket),
                        },
                        new AdaptiveFact
                        {
                            Title = Resource.TitleText,
                            Value = CardHelper.TruncateStringIfLonger(this.ticket.Title, CardHelper.UserTitleMaxLength),
                        },
                        new AdaptiveFact
                        {
                            Title = Resource.DescriptionText,
                            Value = CardHelper.TruncateStringIfLonger(
                                CardHelper.ConvertNullOrEmptyToNotApplicable(this.ticket.Description),
                                CardHelper.UserDescriptionMaxLength),
                        },
                        new AdaptiveFact
                        {
                            Title = Resource.DateCreatedDisplayFactTitle,
                            Value = CardHelper.GetFormattedDateInUserTimeZone(this.ticket.DateCreated, activityLocalTimestamp),
                        },
                    },
                };
            }
            else
            {
                return new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact
                        {
                            Title = Resource.StatusFactTitle,
                            Value = CardHelper.GetUserTicketStatus(this.ticket),
                        },
                        new AdaptiveFact
                        {
                            Title = Resource.TitleText,
                            Value = CardHelper.TruncateStringIfLonger(this.ticket.Title, CardHelper.UserTitleMaxLength),
                        },
                        new AdaptiveFact
                        {
                            Title = Resource.DescriptionText,
                            Value = CardHelper.TruncateStringIfLonger(
                                CardHelper.ConvertNullOrEmptyToNotApplicable(this.ticket.Description),
                                CardHelper.UserDescriptionMaxLength),
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
                };
            }
        }
    }
}