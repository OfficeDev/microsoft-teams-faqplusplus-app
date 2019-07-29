// <copyright file="AskAnExpertCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Ask An Expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class AskAnExpertCard
    {
        private static readonly string CardTemplate;

        static AskAnExpertCard()
        {
            var cardJsonFilePath = Path.Combine(".",  "AdaptiveCards", "AskAnExpertCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Ask an Expert as an Attachment.</returns>
        public static Attachment GetCard()
        {
            var variablesToValues = new Dictionary<string, string>()
            {
                { "cardHeader", Resource.AskAnExpertText1 },
                { "subHeader", Resource.AskAnExpertPlaceholderText },
                { "titleText",  Resource.TitleText },
                { "showcardTitleText",  Resource.ShowCardTitleText },
                { "descriptionText", Resource.DescriptionText },
                { "descriptionPlaceholder", Resource.AskAnExpertPlaceholderText },
                { "askAnExpertButtonText", Resource.AskAnExpertButtonText },
            };
            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
