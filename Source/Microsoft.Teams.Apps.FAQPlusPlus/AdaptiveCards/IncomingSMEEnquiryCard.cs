// <copyright file="IncomingSMEEnquiryCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a question or a feedback through bot.
    /// </summary>
    public class IncomingSMEEnquiryCard
    {
        private static readonly string CardTemplate;

        /// <summary>
        /// Initializes static members of the <see cref="IncomingSMEEnquiryCard"/> class.
        /// </summary>
        static IncomingSMEEnquiryCard()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "IncomingSMEEnquiryCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="incomingTitleText">Title of the user question for the expert.</param>
        /// <param name="incomingSubtitleText">Adaptive card subtitle text based on the user activity type.</param>
        /// <param name="channelAccountDetails">Channel details to which bot post the user question.</param>
        /// <param name="userActivityPayload">User activity type:posting a feedback or asking a question to the expert.</param>
        /// <param name="isStatusAvailable">Flag value for status button- required only for ask an expert scenarios.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string incomingTitleText,
            string incomingSubtitleText,
            TeamsChannelAccount channelAccountDetails,
            UserActivity userActivityPayload,
            bool isStatusAvailable = false)
        {
            var incomingQuestionText = GetQuestionText(userActivityPayload);
            var incomingAnswerText = string.IsNullOrEmpty(userActivityPayload.SMEAnswer) ? "NA" : userActivityPayload.SMEAnswer;
            var smeQuestion = string.IsNullOrEmpty(userActivityPayload.SMEQuestion) ? "NA" : userActivityPayload.SMEQuestion;
            var chatTextButton = string.Format(Resource.ChatTextButton, channelAccountDetails.GivenName);
            var currentDateTime = DateTime.Now.ToString("s") + "Z";
            var variablesToValues = new Dictionary<string, string>()
            {
                { "titleText",  Resource.TitleText },
                { "userTitleValue", userActivityPayload.UserTitleText },
                { "descriptionText", Resource.DescriptionText },
                { "incomingFeedbackText", incomingQuestionText },
                { "kbEntryText", Resource.KBEntryText },
                { "smeAnswer", incomingAnswerText },
                { "questionText", Resource.QuestionText },
                { "smeQuestion", smeQuestion },
                { "dateCreatedDisplayFactTitle", Resource.DateCreatedDisplayFactTitle },

                // TO-DO: need to pass date created value from the previous entity creation method
                { "dateCreatedValue", DateTime.Now.ToString("s") + "Z" },
                { "incomingTitleText", incomingTitleText },
                { "incomingSubtitleText", incomingSubtitleText },
                { "personUpn", channelAccountDetails.Email },
                { "chatTextButton", chatTextButton }
            };
            if (isStatusAvailable)
            {
                variablesToValues.Add("changeStatusButtonText", Resource.ChangeStatusButtonText);
                variablesToValues.Add("openStatusText", Resource.OpenStatusText);
                variablesToValues.Add("assignStatusText", Resource.AssignStatusText);
                variablesToValues.Add("closeStatusText", Resource.CloseStatusText);
                variablesToValues.Add("submitButtonText", Resource.SubmitButtonText);
                variablesToValues.Add("closedFactTitle", Resource.ClosedFactTitle);
                variablesToValues.Add("dateClosedValue", currentDateTime);
                variablesToValues.Add("statusFactTitle", Resource.StatusFactTitle);
                variablesToValues.Add("statusValue", Resource.OpenStatusText);
                return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
            }

            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "IncomingSMEFeedbackCard.json");
            var feedbackCardTemplate = File.ReadAllText(cardJsonFilePath);

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(feedbackCardTemplate, variablesToValues));
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
                return "NA";
            }
        }
    }
}