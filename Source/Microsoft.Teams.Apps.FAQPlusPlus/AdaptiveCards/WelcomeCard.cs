﻿// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Threading.Tasks;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public class WelcomeCard
    {
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

            AdaptiveCard userWelcomeCard = new AdaptiveCard("1.0");
            userWelcomeCard.Body.Add(new AdaptiveTextBlock()
            {
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Size = AdaptiveTextSize.Small,
                Spacing = AdaptiveSpacing.Small,
                Weight = AdaptiveTextWeight.Default,
                Text = welcomeText1,
                Wrap = true
            });

            userWelcomeCard.Body.Add(new AdaptiveTextBlock()
            {
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Size = AdaptiveTextSize.Small,
                Spacing = AdaptiveSpacing.Small,
                Weight = AdaptiveTextWeight.Default,
                Text = messageText1,
                Wrap = true
            });

            userWelcomeCard.Body.Add(new AdaptiveTextBlock()
            {
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Size = AdaptiveTextSize.Small,
                Spacing = AdaptiveSpacing.None,
                Weight = AdaptiveTextWeight.Default,
                Text = welcomeCardBulletText,
                Wrap = true
            });

            userWelcomeCard.Body.Add(new AdaptiveTextBlock()
            {
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Size = AdaptiveTextSize.Small,
                Spacing = AdaptiveSpacing.Small,
                Weight = AdaptiveTextWeight.Default,
                Text = messageText2,
                Wrap = true
            });

            // User take a tour submit action.
            userWelcomeCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = takeATourButtonText,
                Data = Newtonsoft.Json.Linq.JObject.FromObject(
                     new
                     {
                         msteams = new
                         {
                             type = "messageBack",
                             displayText = takeATourButtonText,
                             text = "take a tour"
                         }
                     })
            });

            return CardHelper.GenerateCardAttachment(userWelcomeCard.ToJson());
        }
    }
}
