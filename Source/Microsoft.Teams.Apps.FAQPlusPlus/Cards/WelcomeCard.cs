// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public class WelcomeCard
    {
        /// <summary>
        /// This method will construct the user welcome card when bot is added in personal scope.
        /// </summary>
        /// <param name="welcomeText">Gets welcome text.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static async Task<Attachment> GetCard(string welcomeText)
        {
            AdaptiveCard userWelcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.Small,
                        Text = welcomeText1,
                        Wrap = true
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.TakeATourButtonText,
                        Data = Newtonsoft.Json.Linq.JObject.FromObject(new
                             {
                                 msteams = new
                                 {
                                     type = ActionTypes.MessageBack,
                                     displayText = Resource.TakeATourButtonText,
                                     text = FaqPlusPlusBot.TakeATour
                                 }
                             })
                    }
                }
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = userWelcomeCard,
            };
        }
    }
}
