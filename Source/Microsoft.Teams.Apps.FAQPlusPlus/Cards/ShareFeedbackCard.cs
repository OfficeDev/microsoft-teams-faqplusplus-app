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
    ///  This class process a Share feedback Card function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class ShareFeedbackCard
    {
        /// <summary>
        /// This method will construct the share feedback adaptive card through bot menu.
        /// </summary>
        /// <returns>Feedback as an Attachment.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.FeedbackHeaderText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.FeedbackText1,
                        Wrap = true
                    },
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
                        Id = nameof(SubmitUserRequestPayload.FeedbackUserTitleText),
                        Placeholder = Resource.ShowCardTitleText,
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
                        Id = nameof(SubmitUserRequestPayload.AppFeedback),
                        Placeholder = Resource.FeedbackDescriptionPlaceholderText,
                        IsMultiline = true
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
                            }
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