// <copyright file="ConfirmationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// The class process Confirmation card-upon bot sending the user query to SME channel.
    /// </summary>
    public class ConfirmationCard
    {
        private const string ImageUri = "https://faqplusplus.azurewebsites.net";
        private static readonly string CardTemplate;

        /// <summary>
        /// Initializes static members of the <see cref="ConfirmationCard"/> class.
        /// </summary>
        static ConfirmationCard()
        {
            var cardJsonFilePath = Path.Combine(".",  "AdaptiveCards", "ConfirmationCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>The JSON string for the adaptive card.</returns>
        public static Attachment GetCard(string question)
        {
            var confirmationCardTitleText = Resource.ConfirmationCardTitleText;
            var confirmationAdaptiveCardContent = Resource.ConfirmationAdaptiveCardContent;
            var cardImageUrl = ImageUri + "/content/ShareFeedback.png";

            var variablesToValues = new Dictionary<string, string>()
            {
                { "confirmationCardTitleText", confirmationCardTitleText },
                { "cardImageUrl", cardImageUrl },
                { "confirmationAdaptiveCardContent", confirmationAdaptiveCardContent },
                { "smeQuestion", $"Description :{question}" },
                { "status", $"Status: {Resource.OpenStatusText}" },
                { "date", $"Date Started:{DateTime.UtcNow.ToString()}" }
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}