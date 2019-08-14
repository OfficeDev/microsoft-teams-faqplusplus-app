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
        /// This method will construct the card for ask an expert, when invoked from the bot menu.
        /// </summary>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard()
        {
            return GetCard(false);
        }

        /// <summary>
        /// This method will construct the card for ask an expert, when invoked from the response card.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ResponseCardPayload payload)
        {
            // Pre-populate the description with the user's question
            var description = payload.UserQuestion;
            return GetCard(false, description, payload.UserQuestion, payload.KnowledgeBaseAnswer);
        }

        /// <summary>
        /// This method will construct the card for ask an expert, when invoked from the adaptive card submit action.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(SubmitUserRequestPayload payload)
        {
            return GetCard(true, payload.QuestionForExpert, payload.UserQuestion, payload.SmeAnswer);
        }

        /// <summary>
        /// This method will construct the card for ask an expert bot menu.
        /// </summary>
        /// <param name="showValidationErrors">Determines whether we show validation errors.</param>
        /// <param name="description">User activity text.</param>
        /// <param name="userQuestion">The original text that the user entered.</param>
        /// <param name="knowledgeBaseAnswer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <returns>Ask an expert card.</returns>
        private static Attachment GetCard(bool showValidationErrors = false, string description = null, string userQuestion = null, string knowledgeBaseAnswer = null)
        {
            description = description ?? string.Empty;
            knowledgeBaseAnswer = knowledgeBaseAnswer ?? string.Empty;
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
                                        Text = showValidationErrors ? Resource.MandatoryTitleFieldText : string.Empty,
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
                        Value = description,
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
                            UserQuestion = userQuestion,
                            SmeAnswer = knowledgeBaseAnswer,
                            QuestionForExpert = description,
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