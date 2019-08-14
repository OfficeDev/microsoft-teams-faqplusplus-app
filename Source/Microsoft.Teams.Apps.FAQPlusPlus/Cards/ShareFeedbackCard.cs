// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
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
    ///  This class process a Share feedback function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class ShareFeedbackCard
    {
        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the bot menu.
        /// </summary>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard()
        {
            return GetCard(false);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the response card.
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
        /// This method will construct the card for share feedback, when invoked from the feedback card submit.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(SubmitUserRequestPayload payload)
        {
            return GetCard(true, payload.QuestionForExpert, payload.UserQuestion, payload.SmeAnswer);
        }

        /// <summary>
        /// This method will construct the card  for share feedback bot menu.
        /// </summary>
        /// <param name="showValidationErrors">Flag to determine rating value.</param>
        /// <param name="description">User activity text.</param>
        /// <param name="userQuestion">Question asked by the user to bot.</param>
        /// <param name="knowledgeBaseAnswer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <returns>Share feedback card.</returns>
        private static Attachment GetCard(bool showValidationErrors, string description = "", string userQuestion = "", string knowledgeBaseAnswer = "")
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = !string.IsNullOrWhiteSpace(userQuestion) ? Resource.ResultsFeedbackText : Resource.ShareFeedbackTitleText,
                        Size = AdaptiveTextSize.Large,
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
                                        Text = Resource.FeedbackRatingRequired,
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
                                        Text = showValidationErrors ? Resource.RatingMandatoryText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Wrap = true
                                    }
                                }
                            }
                        },
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = nameof(SubmitUserRequestPayload.FeedbackRatingAction),
                        IsMultiSelect = false,
                        Style = AdaptiveChoiceInputStyle.Compact,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice
                            {
                                Title = Resource.HelpfulRatingText,
                                Value = SubmitUserRequestPayload.HelpfulRatingAction,
                            },
                            new AdaptiveChoice
                            {
                                Title = Resource.NeedsImprovementRatingText,
                                Value = SubmitUserRequestPayload.NeedsImprovementRatingAction,
                            },
                            new AdaptiveChoice
                            {
                                Title = Resource.UnhelpfulRatingText,
                                Value = SubmitUserRequestPayload.UnhelpfulRatingAction,
                            },
                        }
                    },
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.DescriptionText,
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(SubmitUserRequestPayload.QuestionForExpert),
                        Placeholder = Resource.FeedbackDescriptionPlaceholderText,
                        IsMultiline = true,
                        Value = description,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.ShareFeedbackButtonText,
                        Data = new
                        {
                            msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.ShareFeedbackDisplayText,
                                Text = SubmitUserRequestPayload.AppFeedbackAction
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
                Content = shareFeedbackCard,
            };
        }
    }
}