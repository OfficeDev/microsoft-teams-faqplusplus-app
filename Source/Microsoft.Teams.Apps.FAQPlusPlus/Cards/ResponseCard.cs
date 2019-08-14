// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// This method will construct the response card - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="question">Actual question from the QnA Maker service.</param>
        /// <param name="answer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>QnA response card.</returns>
        public static Attachment GetCard(string question, string answer, string userQuestion)
        {
            AdaptiveCard responseCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.ResponseHeaderText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = question,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.AskAnExpertButtonText,
                        Data = new
                        {
                            msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.AskAnExpertDisplayText,
                                Text = FaqPlusPlusBot.AskAnExpert
                            },
                            UserQuestion = userQuestion,
                            SmeAnswer = answer
                        }
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.ShareFeedbackButtonText,
                        Data = new
                        {
                            msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.ShareFeedbackDisplayText,
                                Text = FaqPlusPlusBot.Feedback
                            },
                             UserQuestion = userQuestion,
                             SmeAnswer = answer
                        }
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }
    }
}