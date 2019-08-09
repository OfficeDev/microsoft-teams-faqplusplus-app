// <copyright file="ThankYouCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// The class process Thank You adaptive card-upon bot posting user feedback to SME team.
    /// </summary>
    public class ThankYouCard
    {
        /// <summary>
        /// This method will send thank you adaptive card to user upon posting feedback to SME team.
        /// </summary>
        /// <returns>The JSON string for the adaptive card.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard thankYouCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.ThankYouCardTitleText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Text = Resource.ThankYouCardContent,
                        Wrap = true
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = thankYouCard,
            };
        }
    }
}