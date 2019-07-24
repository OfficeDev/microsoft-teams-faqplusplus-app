// <copyright file="ThankYouAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// The class process Thank You adaptive card-upon bot posting user feedback to SME team.
    /// </summary>
    public class ThankYouAdaptiveCard
    {
        private const string ImageUri = "https://faqplusplus.azurewebsites.net";
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
            var cardImageUrl = ImageUri + "/content/ShareFeedback.png";
            var thankYouAdaptiveCardTitleText = Resource.ThankYouAdaptiveCardTitleText;
            var thankYouAdaptiveCardContent = Resource.ThankYouAdaptiveCardContent;

            var variablesToValues = new Dictionary<string, string>()
            {
                { "thankYouAdaptiveCardTitleText", thankYouAdaptiveCardTitleText },
                { "cardImageUrl", cardImageUrl },
                { "thankYouAdaptiveCardContent", thankYouAdaptiveCardContent },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}