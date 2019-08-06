// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// This method will construct the response card when user asks a question to QnA maker through bot.
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
                                    Text = Resource.MandatoryFieldText,
                                    Color = AdaptiveTextColor.Attention,
                                    Spacing = AdaptiveSpacing.Small,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Placeholder = Resource.ShowCardTitleText,
                                    Id = nameof(SubmitUserRequestPayload.QuestionUserTitleText),
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
                                    Id = nameof(SubmitUserRequestPayload.QuestionForExpert),
                                    Value = userQuestion,
                                    IsMultiline = true
                                }
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Title = Resource.SubmitButtonText,
                                    Data = Newtonsoft.Json.Linq.JObject.FromObject(new
                                    {
                                        msteams = new
                                        {
                                            type = ActionTypes.MessageBack,
                                            displayText = Resource.AskAnExpertDisplayText,
                                            text = SubmitUserRequestPayload.QuestionForExpertAction
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
                                    Text = Resource.MandatoryFieldText,
                                    Color = AdaptiveTextColor.Attention,
                                    Spacing = AdaptiveSpacing.Small,
                                    Wrap = true
                                },
                                new AdaptiveTextInput
                                {
                                    Placeholder = Resource.ShowCardTitleText,
                                    Id = nameof(SubmitUserRequestPayload.FeedbackUserTitleText),
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
                                    Id = nameof(SubmitUserRequestPayload.ResultsFeedback),
                                    IsMultiline = true,
                                    Placeholder = Resource.Resultsfeedbackdetails
                                }
                            },
                            Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
                                    Title = Resource.SubmitButtonText,
                                    Data = Newtonsoft.Json.Linq.JObject.FromObject(new
                                    {
                                        msteams = new
                                        {
                                            type = ActionTypes.MessageBack,
                                            displayText = Resource.ShareFeedbackDisplayText,
                                            text = SubmitUserRequestPayload.ResultsFeedbackAction
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
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }
    }
}