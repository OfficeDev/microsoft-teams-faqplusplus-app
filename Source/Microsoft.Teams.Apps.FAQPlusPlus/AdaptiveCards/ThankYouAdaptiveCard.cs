// <copyright file="ThankYouAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// The class process Thank You adaptive card-upon bot posting user feedback to SME team.
    /// </summary>
    public class ThankYouAdaptiveCard
    {
        /// <summary>
        /// This method will send thank you adaptive card to user upon posting feedback to SME team.
        /// </summary>
        /// <returns>The JSON string for the adaptive card.</returns>
        [System.Obsolete]
        public static Attachment GetCard()
        {
            AdaptiveCard thankYouCard = new AdaptiveCard("1.0");

            thankYouCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.ThankYouAdaptiveCardTitleText,
                Wrap = true
            });
            thankYouCard.Body.Add(new AdaptiveTextBlock()
            {
                Spacing = AdaptiveSpacing.Medium,
                Text = Resource.ThankYouAdaptiveCardContent,
                Wrap = true
            });

            return CardHelper.GenerateCardAttachment(thankYouCard.ToJson());
        }
    }
}