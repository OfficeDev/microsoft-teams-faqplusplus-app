// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public class WelcomeCard
    {
        private static readonly string CardTemplate;

        static WelcomeCard()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "WelcomeCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="welcomeText">Gets welcome text.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static async Task<Attachment> GetCard(string welcomeText)
        {
            string[] welcomeTextValues = welcomeText.Split(';');
            var welcomeText1 = welcomeTextValues[0];
            var messageText1 = welcomeTextValues[1];
            var welcomeCardBulletText = welcomeTextValues[2];
            var messageText2 = welcomeTextValues[3];
            var takeATourButtonText = welcomeTextValues[4];
            var variablesToValues = new Dictionary<string, string>()
            {
                { "welcomeText1", welcomeText1 },
                { "messageText1", messageText1 },
                { "welcomeCardBulletText", welcomeCardBulletText },
                { "messageText2", messageText2 },
                { "takeATourButtonText", takeATourButtonText },
            };
            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
