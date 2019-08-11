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
        /// This method will construct the share feedback adaptive card through bot menu.
        /// </summary>
        /// <param name="isRatingRequired">Flag to determine rating value.</param>
        /// <param name="userQuestionText">Question asked by the user to bot.</param>
        /// <param name="qnaAswerText">The response that the bot retrieves after querying the knowledge base.</param>
        /// <returns>Share feedback card.</returns>
        public static Attachment GetCard(bool isRatingRequired = false, string userQuestionText = "", string qnaAswerText = "")
        {
            string cardTitleText = !string.IsNullOrWhiteSpace(userQuestionText) ? Resource.ResultsFeedbackText : Resource.ShareFeedbackTitleText;

            AdaptiveCard shareFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = cardTitleText,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Resource.FeedbackRatingRequired,
                        Wrap = true
                    },
                     new AdaptiveTextBlock
                    {
                       Text = isRatingRequired ? Resource.RatingMandatoryFieldText : string.Empty,
                       Color = AdaptiveTextColor.Attention,
                       HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                       Wrap = true
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
                        Value = !string.IsNullOrWhiteSpace(userQuestionText) ? userQuestionText : string.Empty,
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
                           UserQuestion = !string.IsNullOrWhiteSpace(userQuestionText) ? userQuestionText : string.Empty,
                           SmeAnswer = !string.IsNullOrWhiteSpace(qnaAswerText) ? qnaAswerText : string.Empty,
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