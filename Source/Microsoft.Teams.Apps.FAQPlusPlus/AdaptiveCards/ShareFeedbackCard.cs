// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process a ShareFeedback Card function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class ShareFeedbackCard
    {
        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Feedback as an Attachment.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard("1.0");
            shareFeedbackCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.FeedbackHeaderText,
                Wrap = true
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.FeedbackText1,
                Wrap = true
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.TitleText,
                Wrap = true
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.MandatoryFieldText,
                Color = AdaptiveTextColor.Attention,
                Spacing = AdaptiveSpacing.Small,
                Wrap = true
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "feedbackUserTitleText",
                Placeholder = Resource.ShowCardTitleText,
                IsMultiline = false
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.DescriptionText,
                Wrap = true
            });

            shareFeedbackCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "AppFeedback",
                Placeholder = Resource.FeedbackDescriptionPlaceholderText,
                IsMultiline = true
            });

            shareFeedbackCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resource.ShareFeedbackButtonText,
                Data = Newtonsoft.Json.Linq.JObject.FromObject(
                new
                {
                    msteams = new
                    {
                        type = "messageBack",
                        displayText = Resource.ShareFeedbackDisplayText,
                        text = "AppFeedback"
                    }
                })
            });
            return CardHelper.GenerateCardAttachment(shareFeedbackCard.ToJson());
        }
    }
}