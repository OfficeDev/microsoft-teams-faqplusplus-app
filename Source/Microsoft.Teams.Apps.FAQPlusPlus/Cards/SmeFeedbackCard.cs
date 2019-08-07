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
        /// <param name="titleText">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccountDetails">Details of the user submitting the feedback.</param>
        /// <param name="userActivityPayload">Payload from the feedback submission.</param>
        /// <param name="localTimeStamp">Local time stamp of the user activity.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateAppFeedbackCard(
            string titleText,
            TeamsChannelAccount userAccountDetails,
            SubmitUserRequestPayload userActivityPayload,
            DateTimeOffset? localTimeStamp)
        {
            var cardSubtitleText = string.Format(Resource.FeedbackSubHeaderText, userAccountDetails.Name, Resource.AppFeedbackText);
            return GetCard(Resource.AppFeedbackText, titleText, cardSubtitleText, userAccountDetails, userActivityPayload.SmeAnswer, userActivityPayload.UserQuestion, userActivityPayload.AppFeedback, localTimeStamp);
        }

        /// <summary>
        /// Create a card that represents result feedback.
        /// </summary>
        /// <param name="titleText">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccountDetails">Details of the user submitting the feedback.</param>
        /// <param name="userActivityPayload">Payload from the feedback submission.</param>
        /// <param name="localTimeStamp">Local time stamp of the user activity.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateResultFeedbackCard(
            string titleText,
            TeamsChannelAccount userAccountDetails,
            SubmitUserRequestPayload userActivityPayload,
            DateTimeOffset? localTimeStamp)
        {
            var cardSubtitleText = string.Format(Resource.FeedbackSubHeaderText, userAccountDetails.Name, Resource.ResultsFeedbackText);
            return GetCard(Resource.ResultsFeedbackText, titleText, cardSubtitleText, userAccountDetails, userActivityPayload.SmeAnswer, userActivityPayload.UserQuestion, userActivityPayload.ResultsFeedback, localTimeStamp);
        }

        /// <summary>
        /// This method will construct the adaptive card that is sent to the SME team for feedback activity.
        /// </summary>
        /// <param name="cardHeaderText">Title of the user activity-for feedback or ask an expert.</param>
        /// <param name="titleText">Actual title text entered by the user for the given scenario.</param>
        /// <param name="cardSubtitleText">Adaptive card subtitle text based on the user activity type.</param>
        /// <param name="channelAccountDetails">Channel details to which bot post the user question.</param>
        /// <param name="kbAnswer">Answer from the Knowledgebase.</param>
        /// <param name="userQuestion">Question asked by the user to the bot.</param>
        /// <param name="descriptionText">Description entered by the user.</param>
        /// <param name="localTimeStamp">Local time stamp of the user activity.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string cardHeaderText,
            string titleText,
            string cardSubtitleText,
            TeamsChannelAccount channelAccountDetails,
            string kbAnswer,
            string userQuestion,
            string descriptionText,
            DateTimeOffset? localTimeStamp)
        {
            var chatTextButton = string.Format(Resource.ChatTextButton, channelAccountDetails.GivenName);
            kbAnswer = CardHelper.TruncateStringIfLonger(kbAnswer, CardHelper.KbAnswerMaxLength);

            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = cardHeaderText,
                        Color = AdaptiveTextColor.Attention,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = cardSubtitleText,
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
                                Value = titleText,
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.DescriptionText,
                                Value = descriptionText,
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.KBEntryFactTitle,
                                Value = CardHelper.ValidateTextIsNullorEmpty(kbAnswer),
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.QuestionAskedFactTitle,
                                Value = CardHelper.ValidateTextIsNullorEmpty(userQuestion),
                             },

                             new AdaptiveFact
                             {
                                Title = Resource.DateCreatedDisplayFactTitle,
                                Value = CardHelper.GetLocalTimeStamp(localTimeStamp),
                             },
                         },
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = chatTextButton,
                        UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={channelAccountDetails.Email}"
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