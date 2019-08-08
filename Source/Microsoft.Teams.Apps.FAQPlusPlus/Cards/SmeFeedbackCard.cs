// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a feedback through bot menu or from response card.
    /// </summary>
    public class SmeFeedbackCard
    {
        /// <summary>
        /// Create a card that represents application feedback.
        /// </summary>
        /// <param name="userEnteredTitle">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccount">Details of the user submitting the feedback.</param>
        /// <param name="activityPayload">Payload from the feedback submission.</param>
        /// <param name="activityLocalTimestamp">Local timestamp of the user activity.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateAppFeedbackCard(
            string userEnteredTitle,
            TeamsChannelAccount userAccount,
            SubmitUserRequestPayload activityPayload,
            DateTimeOffset? activityLocalTimestamp)
        {
            var cardSubtitle = string.Format(Resource.FeedbackSubHeaderText, userAccount.Name, Resource.AppFeedbackText);
            return GetCard(Resource.AppFeedbackText, cardSubtitle, userEnteredTitle, activityPayload.AppFeedback, activityPayload.SmeAnswer, activityPayload.UserQuestion, userAccount, activityLocalTimestamp);
        }

        /// <summary>
        /// Create a card that represents result feedback.
        /// </summary>
        /// <param name="userEnteredTitle">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccount">Details of the user submitting the feedback.</param>
        /// <param name="activityPayload">Payload from the feedback submission.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of the user activity.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateResultFeedbackCard(
            string userEnteredTitle,
            TeamsChannelAccount userAccount,
            SubmitUserRequestPayload activityPayload,
            DateTimeOffset? activityLocalTimestamp)
        {
            var cardSubtitle = string.Format(Resource.FeedbackSubHeaderText, userAccount.Name, Resource.ResultsFeedbackText);
            return GetCard(Resource.ResultsFeedbackText, cardSubtitle, userEnteredTitle, activityPayload.ResultsFeedback, activityPayload.SmeAnswer, activityPayload.UserQuestion, userAccount, activityLocalTimestamp);
        }

        /// <summary>
        /// This method will construct the adaptive card that is sent to the SME team for feedback activity.
        /// </summary>
        /// <param name="title">Title of the user activity-for feedback or ask an expert.</param>
        /// <param name="subtitle">Adaptive card subtitle text based on the user activity type.</param>
        /// <param name="userEnteredTitle">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userEnteredDescription">Description entered by the user.</param>
        /// <param name="kbAnswer">Answer from the Knowledgebase.</param>
        /// <param name="userOriginalQuestion">Question asked by the user to the bot.</param>
        /// <param name="userAccount">Channel details to which bot post the user question.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of the user activity.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string title,
            string subtitle,
            string userEnteredTitle,
            string userEnteredDescription,
            string kbAnswer,
            string userOriginalQuestion,
            TeamsChannelAccount userAccount,
            DateTimeOffset? activityLocalTimestamp)
        {
            var chatTextButton = string.Format(Resource.ChatTextButton, userAccount.GivenName);
            kbAnswer = CardHelper.TruncateStringIfLonger(kbAnswer, CardHelper.KbAnswerMaxLength);

            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = title,
                        Color = AdaptiveTextColor.Attention,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = subtitle,
                        Wrap = true
                    },
                    new AdaptiveFactSet
                    {
                         Facts = new List<AdaptiveFact>
                         {
                             new AdaptiveFact
                             {
                                Title = Resource.StatusFactTitle,
                                Value = Resource.OpenStatusTitle,
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.TitleText,
                                Value = userEnteredTitle,
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.DescriptionText,
                                Value = userEnteredDescription,
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.KBEntryFactTitle,
                                Value = CardHelper.ConvertNullOrEmptyToNotApplicable(kbAnswer),
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.QuestionAskedFactTitle,
                                Value = CardHelper.ConvertNullOrEmptyToNotApplicable(userOriginalQuestion),
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.DateCreatedDisplayFactTitle,
                                Value = CardHelper.GetFormattedDateInUserTimeZone(DateTime.UtcNow, activityLocalTimestamp),
                             },
                         },
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = chatTextButton,
                        UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userAccount.UserPrincipalName)}"
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = smeFeedbackCard,
            };
        }
    }
}