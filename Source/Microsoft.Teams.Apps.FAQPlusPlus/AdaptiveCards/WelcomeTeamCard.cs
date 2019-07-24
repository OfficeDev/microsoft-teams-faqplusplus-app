// <copyright file="WelcomeTeamCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process  Welcome Card when installed in Team scope.
    /// </summary>
    public static class WelcomeTeamCard
    {
        private static readonly string CardTemplate;

        static WelcomeTeamCard()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "WelcomeTeamCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="botDisplayName">Name of the bot.</param>
        /// <param name="teamName">Name of the team to which bot is added to. </param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string botDisplayName, string teamName = null)
        {
            var welcomeTeamCardTitleText = string.Format(Resource.WelcomeTeamCardTitleText, teamName);
            var welcomeTeamCardContent = string.Format(Resource.WelcomeTeamCardContent, botDisplayName, teamName);
            var takeATeamTourButtonText = Resource.TakeATeamTourButtonText;
            var variablesToValues = new Dictionary<string, string>()
            {
                { "welcomeTeamCardTitleText", welcomeTeamCardTitleText },
                { "welcomeTeamCardContent", welcomeTeamCardContent },
                { "takeATeamTourButtonText", takeATeamTourButtonText },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
