// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    public class SmeTicketCard
    {
        private TicketEntity ticketModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public SmeTicketCard(TicketEntity ticket)
        {
            this.ticketModel = ticket;
        }

        /// <summary>
        /// Method to generate the adaptive card.
        /// </summary>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Yahtzee!",
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        Title = "Status",
                        Card = new AdaptiveCard
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = "Making sure to have the dropdown here"
                                },
                            },
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