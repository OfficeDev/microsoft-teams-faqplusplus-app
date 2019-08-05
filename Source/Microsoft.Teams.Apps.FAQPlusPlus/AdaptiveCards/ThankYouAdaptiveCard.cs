// <copyright file="ThankYouAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// The class process Thank You adaptive card-upon bot posting user feedback to SME team.
    /// </summary>
    public class ThankYouAdaptiveCard
    {
        private static readonly string CardTemplate;

        /// <summary>
        /// Initializes static members of the <see cref="ThankYouAdaptiveCard"/> class.
        /// </summary>
        static ThankYouAdaptiveCard()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "ThankYouAdaptiveCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>The JSON string for the adaptive card.</returns>
        public static Attachment GetCard()
        {
            var thankYouAdaptiveCardTitleText = Resource.ThankYouAdaptiveCardTitleText;
            var thankYouAdaptiveCardContent = Resource.ThankYouAdaptiveCardContent;
            var todaysDate = DateTime.Now.ToString("s") + "Z";
            var closedDate = Resource.NonApplicableString;

            var variablesToValues = new Dictionary<string, string>()
            {
                { "thankYouAdaptiveCardTitleText", thankYouAdaptiveCardTitleText },
                { "thankYouAdaptiveCardContent", thankYouAdaptiveCardContent },
                { "todaysDate", todaysDate },
                { "closedDate", closedDate },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}