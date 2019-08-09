// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;

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
        public static Attachment GetCard(string welcomeText)
        {
            string[] welcomeTextValues = welcomeText.Split(';');
            var welcomeText1 = welcomeTextValues[0];
            var messageText1 = welcomeTextValues[1];
            var welcomeCardBulletText = welcomeTextValues[2];
            var messageText2 = welcomeTextValues[3];
            var takeATourButtonText = welcomeTextValues[4];

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
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.Small,
                        Text = messageText1,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.None,
                        Text = welcomeCardBulletText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.Small,
                        Text = messageText2,
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = takeATourButtonText,
                        Data = new
                        {
                            msteams = new CardAction
                            {
                              Type = ActionTypes.MessageBack,
                              DisplayText = takeATourButtonText,
                              Text = FaqPlusPlusBot.TakeATour
                            }
                        },
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