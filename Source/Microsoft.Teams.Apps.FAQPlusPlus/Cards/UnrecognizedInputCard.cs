// <copyright file="UnrecognizedInputCard.cs" company="Microsoft">
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
    ///  This class handles unrecognized input sent by the user-asking random question to bot.
    /// </summary>
    public static class UnrecognizedInputCard
    {
        /// <summary>
        /// This method will construct the adaptive card when unrecognized input is sent by the user.
        /// </summary>
        /// <param name="question">The question that the user asks the bot.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string question)
        {
            {
                AdaptiveCard unrecognizedInputCard = new AdaptiveCard("1.0")
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = Resource.CustomMessage,
                            Wrap = true
                        }
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveShowCardAction
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
                                        Value = question,
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
                                                text = "QuestionForExpert"
                                            },
                                            UserQuestion = question,
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
                    Content = unrecognizedInputCard,
                };
            }
        }
    }
}
