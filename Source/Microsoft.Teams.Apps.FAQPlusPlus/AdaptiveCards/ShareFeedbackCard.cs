// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///  This class process a ShareFeedback Card function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class ShareFeedbackCard
    {
        private static readonly string CardTemplate;

        static ShareFeedbackCard()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "ShareFeedbackCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Feedback as an Attachment.</returns>
        public static Attachment GetCard()
        {
            var variablesToValues = new Dictionary<string, string>()
            {
                { "cardHeader", Resource.FeedbackHeaderText },
                { "subHeader", Resource.FeedbackText1 },
                { "titleText",  Resource.TitleText },
                { "mandatoryFieldText", Resource.MandatoryFieldText },
                { "showcardTitleText",  Resource.ShowCardTitleText },
                { "descriptionText", Resource.DescriptionText },
                { "descriptionPlaceholder", Resource.FeedbackDescriptionPlaceholderText },
                { "shareFeedbackButtonText", Resource.ShareFeedbackButtonText },
                { "shareFeedbackDisplayText", Resource.ShareFeedbackDisplayText },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
