// <copyright file="AskAnExpertCard.cs" company="Microsoft">
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
    ///  This class process Ask an expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class AskAnExpertCard
    {
        /// <summary>
        /// This method will construct the card for ask an expert bot menu.
        /// </summary>
        /// <param name="isTitleMandatory">Flag to determine title value.</param>
        /// <param name="userQuestionText">Question asked by the user to bot.</param>
        /// <param name="userDescriptionText">User activity text.</param>
        /// <param name="qnaAswerText">The response that the bot retrieves after querying the knowledge base.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(bool isTitleMandatory = false, string userQuestionText = "", string userDescriptionText = "", string qnaAswerText = "")
        {
            userDescriptionText = userDescriptionText ?? string.Empty;
            userQuestionText = userQuestionText ?? string.Empty;
            qnaAswerText = qnaAswerText ?? string.Empty;
            AdaptiveCard askAnExpertCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.AskAnExpertText1,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Resource.AskAnExpertSubheaderText,
                        Wrap = true
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Resource.TitleRequiredText,
                                        Wrap = true
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = isTitleMandatory ? Resource.MandatoryTitleFieldText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Wrap = true
                                    }
                                }
                            }
                        },
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(SubmitUserRequestPayload.QuestionUserTitleText),
                        Placeholder = Resource.ShowCardTitleText,
                        IsMultiline = false,
                        Spacing = AdaptiveSpacing.Small
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
                        Placeholder = Resource.AskAnExpertPlaceholderText,
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = string.IsNullOrWhiteSpace(userDescriptionText) ? userQuestionText : userDescriptionText,
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
                                Text = SubmitUserRequestPayload.QuestionForExpertAction
                            },
                            UserQuestion = !string.IsNullOrWhiteSpace(userQuestionText) ? userQuestionText : string.Empty,
                            SmeAnswer = !string.IsNullOrWhiteSpace(qnaAswerText) ? qnaAswerText : string.Empty,
                            QuestionForExpert = !string.IsNullOrWhiteSpace(userDescriptionText) ? userDescriptionText : string.Empty,
                        },
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = askAnExpertCard,
            };
        }
    }
}