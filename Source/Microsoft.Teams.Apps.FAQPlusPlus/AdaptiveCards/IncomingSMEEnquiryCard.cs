// <copyright file="IncomingSMEEnquiryCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a question or a feedback through bot.
    /// </summary>
    public class IncomingSMEEnquiryCard
    {
        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="incomingTitleText">Title of the user activity-for feedback or ask an expert.</param>
        /// <param name="incomingTitleValue">Actual title text entered by the user for the given scenario.</param>
        /// <param name="incomingSubtitleText">Adaptive card subtitle text based on the user activity type.</param>
        /// <param name="channelAccountDetails">Channel details to which bot post the user question.</param>
        /// <param name="userActivityPayload">User activity type:posting a feedback or asking a question to the expert.</param>
        /// <param name="isStatusAvailable">Flag value for status button- required only for ask an expert scenarios.</param>
        /// <returns>The card JSON string.</returns>
        [Obsolete]
        public static Attachment GetCard(
            string incomingTitleText,
            string incomingTitleValue,
            string incomingSubtitleText,
            TeamsChannelAccount channelAccountDetails,
            UserActivity userActivityPayload,
            bool isStatusAvailable = false)
        {
            var incomingQuestionText = GetQuestionText(userActivityPayload);
            var incomingAnswerText = string.IsNullOrEmpty(userActivityPayload.SmeAnswer) ? Resource.NotApplicable : userActivityPayload.SmeAnswer;
            var userQuestion = string.IsNullOrEmpty(userActivityPayload.UserQuestion) ? Resource.NotApplicable : userActivityPayload.UserQuestion;
            var chatTextButton = string.Format(Resource.ChatTextButton, channelAccountDetails.GivenName);
            if (incomingAnswerText.Length > 500)
            {
                incomingAnswerText = incomingAnswerText.Substring(0, 500) + "...";
            }

            var currentDateTime = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ");

            AdaptiveCard incomingSmeCard = new AdaptiveCard();
            incomingSmeCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = incomingTitleText,
                Color = AdaptiveTextColor.Attention,
                Size = AdaptiveTextSize.Medium
            });
            incomingSmeCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = incomingSubtitleText,
                Wrap = true
            });
            incomingSmeCard.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Title = chatTextButton,
                UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={channelAccountDetails.Email}"
            });
            var factSetList = new List<AdaptiveFact>()
                {
                    GetAdaptiveFact(Resource.StatusFactTitle, Resource.OpenStatusValue),
                    GetAdaptiveFact(Resource.TitleText, incomingTitleValue),
                    GetAdaptiveFact(Resource.DescriptionText, incomingQuestionText),
                    GetAdaptiveFact(Resource.KBEntryText, incomingAnswerText),
                    GetAdaptiveFact(Resource.QuestionText, userQuestion),
                    GetAdaptiveFact(Resource.DateCreatedDisplayFactTitle, "{{DATE(" + currentDateTime + ", SHORT)}} {{TIME(" + currentDateTime + ")}}"),
                };
            if (isStatusAvailable)
            {
                factSetList.Add(GetAdaptiveFact(Resource.ClosedFactTitle, Resource.NotApplicable));
                AdaptiveCard showCard = new AdaptiveCard();
                showCard.Title = Resource.ChangeStatusButtonText;
                showCard.Body.Add(new AdaptiveChoiceSetInput()
                {
                    Id = "statuscode",
                    Style = AdaptiveChoiceInputStyle.Compact,
                    IsMultiSelect = false,
                    Value = "1",
                    Choices = new List<AdaptiveChoice>()
                   {
                       GetChoiceSet(Resource.AssignStatusText, "1"),
                       GetChoiceSet(Resource.CloseStatusText, "2")
                   }
                });
                showCard.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = Resource.SubmitButtonText
                });
                incomingSmeCard.Actions.Add(new AdaptiveShowCardAction()
                {
                    Title = Resource.ChangeStatusButtonText,
                    Card = showCard
                });
                incomingSmeCard.Body.Add(new AdaptiveFactSet() { Facts = factSetList });
                return CardHelper.GenerateCardAttachment(incomingSmeCard.ToJson());
            }

            incomingSmeCard.Body.Add(new AdaptiveFactSet() { Facts = factSetList });
            return CardHelper.GenerateCardAttachment(incomingSmeCard.ToJson());
        }

        private static AdaptiveFact GetAdaptiveFact(string title, string value)
        {
            return new AdaptiveFact()
            {
                Title = title,
                Value = value
            };
        }

        private static AdaptiveChoice GetChoiceSet(string title, string value)
        {
            return new AdaptiveChoice()
            {
                Title = title,
                Value = value
            };
        }

        private static string GetQuestionText(UserActivity userActivityPayload)
        {
            if (!string.IsNullOrEmpty(userActivityPayload.QuestionForExpert))
            {
                return userActivityPayload.QuestionForExpert;
            }
            else if (!string.IsNullOrEmpty(userActivityPayload.AppFeedback))
            {
                return userActivityPayload.AppFeedback;
            }
            else if (!string.IsNullOrEmpty(userActivityPayload.ResultsFeedback))
            {
                return userActivityPayload.ResultsFeedback;
            }
            else
            {
                return Resource.NotApplicable;
            }
        }
    }
}