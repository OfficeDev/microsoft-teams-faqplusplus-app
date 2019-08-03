// <copyright file="WelcomeTeamCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process  Welcome Card when installed in Team scope.
    /// </summary>
    public static class WelcomeTeamCard
    {
        /// <summary>
        /// This method will construct the adaptive card used to welcome a team when bot is added to the team.
        /// </summary>
        /// <param name="botDisplayName">Name of the bot.</param>
        /// <param name="teamName">Name of the team to which bot is added to. </param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string botDisplayName, string teamName = null)
        {
            var welcomeTeamCardTitleText = string.Format(Resource.WelcomeTeamCardTitleText, teamName);
            var welcomeTeamCardContent = string.Format(Resource.WelcomeTeamCardContent, botDisplayName, teamName);
            AdaptiveCard teamWelcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        Text = welcomeTeamCardTitleText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = welcomeTeamCardContent,
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    // Team- take a tour submit action.
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.TakeATeamTourButtonText,
                        Data = Newtonsoft.Json.Linq.JObject.FromObject(
                            new
                             {
                                 msteams = new
                                 {
                                     type = "messageBack",
                                     displayText = Resource.TakeATeamTourButtonText,
                                     text = "team tour"
                                 }
                             })
                    }
                }
            };
            return CardHelper.GenerateCardAttachment(teamWelcomeCard.ToJson());
        }
    }
}
