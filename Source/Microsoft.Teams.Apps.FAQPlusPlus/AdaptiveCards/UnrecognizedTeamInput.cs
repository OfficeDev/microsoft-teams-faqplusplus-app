// <copyright file="UnrecognizedTeamInput.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the team memeber-sending random text to bot.
    /// </summary>
    public class UnrecognizedTeamInput
    {
        private static readonly string CardTemplate;

        static UnrecognizedTeamInput()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "UnrecognizedTeamInput.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard()
        {
            var teamcustomMessage = Resource.TeamCustomMessage;
            var takeATeamTourButtonText = Resource.TakeATeamTourButtonText;
            var variablesToValues = new Dictionary<string, string>()
            {
                { "teamCustomMessage", teamcustomMessage },
                { "takeATeamTourButtonText", takeATeamTourButtonText },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
