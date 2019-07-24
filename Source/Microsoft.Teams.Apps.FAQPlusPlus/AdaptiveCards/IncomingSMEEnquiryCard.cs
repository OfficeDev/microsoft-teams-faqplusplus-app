﻿// <copyright file="IncomingSMEEnquiryCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
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
            var cardJsonFilePath = Path.Combine(".",  "AdaptiveCards", "IncomingSMEEnquiryCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="feedbackType">The feedback type - in this case, it's Ask an Expert.</param>
        /// <param name="fullName">The person asking the question.</param>
        /// <param name="personName">Name of the person asking the question.</param>
        /// <param name="personEmail">Email of the Person asking the question.</param>
        /// <param name="incomingFeedbackText">User request- question for expert or providing feedback.</param>
        /// <param name="incomingQuestionText">User requested  question for expert.</param>
        /// <param name="incomingAnswerText">Pre filled response from the QnA maker for  question by the user.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string feedbackType,
            string fullName,
            string personName,
            string personEmail,
            string incomingFeedbackText,
            string incomingQuestionText = "",
            string incomingAnswerText = "")
        {
            var incomingTitleText = feedbackType;
            var incomingSubtitleText = string.Empty;
            if (feedbackType == "Question For Expert")
            {
                incomingSubtitleText = string.Format(Resource.QuestionForExpertSubHeaderText, personName);
            }
            else
            {
               incomingSubtitleText = string.Format(Resource.IncomingFeedbackSubHeaderText, personName, feedbackType);
            }

            incomingQuestionText = incomingQuestionText == string.Empty ? string.Empty : $"Question: {incomingQuestionText}";
            incomingAnswerText = incomingAnswerText == string.Empty ? string.Empty : $"Answer: {incomingAnswerText}";
            incomingFeedbackText = feedbackType == "Question For Expert" ? $"Question: {incomingFeedbackText}" : $"Feedback: {incomingFeedbackText}";
            var chatTextButton = string.Format(Resource.ChatTextButton, personName);
            var statusShowCardHeader = Resource.StatusShowCardHeader;
            var openStatusText = Resource.OpenStatusText;
            var assignStatusText = Resource.AssignStatusText;
            var closeStatusText = Resource.CloseStatusText;
            var submitButtonText = Resource.SubmitButtonText;
            var variablesToValues = new Dictionary<string, string>()
            {
                { "incomingTitleText", incomingTitleText },
                { "incomingSubtitleText", incomingSubtitleText },
                { "incomingQuestionText", incomingQuestionText },
                { "incomingAnswerText", incomingAnswerText },
                { "incomingFeedbackText", incomingFeedbackText },
                { "personUpn", personEmail },
                { "chatTextButton", chatTextButton },
                { "statusShowCardHeader", statusShowCardHeader },
                { "openStatusText", openStatusText },
                { "assignStatusText", assignStatusText },
                { "closeStatusText", closeStatusText },
                { "submitButtonText", submitButtonText },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}