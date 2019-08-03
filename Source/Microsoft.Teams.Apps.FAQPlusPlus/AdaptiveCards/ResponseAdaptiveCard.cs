// <copyright file="ResponseAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseAdaptiveCard
    {
        /// <summary>
        /// This method will construct the response card when user asks a question to Qna maker through bot.
        /// </summary>
        /// <param name="question">Actual question from the QnA maker service.</param>
        /// <param name="answer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>Card attachment as Json string.</returns>
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
                    new AdaptiveShowCardAction()
                    {
                        Title = Resource.AskAnExpertButtonText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Text = Resource.TitleText,
                                    Wrap = true
                                },
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Medium,
                                    Text = Resource.MandatoryFieldText,
                                    Color = AdaptiveTextColor.Attention,
                                    Spacing = AdaptiveSpacing.Small,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Placeholder = Resource.ShowCardTitleText,
                                    Id = "questionUserTitleText",
                                    IsMultiline = false
                                },
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Text = Resource.DescriptionText,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Id = "questionForExpert",
                                    Value = userQuestion,
                                    IsMultiline = true
                                }
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Title = Resource.SubmitButtonText,
                                    Data = Newtonsoft.Json.Linq.JObject.FromObject(
                                    new
                                    {
                                        msteams = new
                                        {
                                            type = "messageBack",
                                            displayText = Resource.AskAnExpertDisplayText,
                                            text = "QuestionForExpert"
                                        },
                                        UserQuestion = userQuestion,
                                        SmeAnswer = answer
                                    })
                                }
                            }
                        }
                    },
                    new AdaptiveShowCardAction
                    {
                        Title = Resource.ShareFeedbackButtonText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Text = Resource.TitleText,
                                    Wrap = true
                                },
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Medium,
                                    Text = Resource.MandatoryFieldText,
                                    Color = AdaptiveTextColor.Attention,
                                    Spacing = AdaptiveSpacing.Small,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Placeholder = Resource.ShowCardTitleText,
                                    Id = "feedbackUserTitleText",
                                    IsMultiline = false
                                },
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Text = Resource.DescriptionText,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Id = "ResultsFeedback",
                                    IsMultiline = true,
                                    Placeholder = Resource.Resultsfeedbackdetails
                                }
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Title = Resource.SubmitButtonText,
                                    Data = Newtonsoft.Json.Linq.JObject.FromObject(
                                    new
                                    {
                                        msteams = new
                                        {
                                            type = "messageBack",
                                            displayText = Resource.ShareFeedbackDisplayText,
                                            text = "ResultsFeedback"
                                        },
                                        UserQuestion = userQuestion,
                                        SmeAnswer = answer
                                    })
                                }
                            }
                        }
                    }
                }
            };
            return CardHelper.GenerateCardAttachment(responseCard.ToJson());
        }
    }
}