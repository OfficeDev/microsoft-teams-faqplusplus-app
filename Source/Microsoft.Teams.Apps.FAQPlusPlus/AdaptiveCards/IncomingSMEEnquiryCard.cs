// <copyright file="IncomingSMEEnquiryCard.cs" company="Microsoft">
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
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "IncomingSMEEnquiryCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="feedbackType">The feedback type - in this case, it's Ask an Expert.</param>
        /// <param name="personName">Name of the person asking the question.</param>
        /// <param name="personEmail">Email of the Person asking the question.</param>
        /// <param name="incomingFeedbackText">User request- question for expert or providing feedback.</param>
        /// <param name="incomingQuestionText">User requested  question for expert.</param>
        /// <param name="incomingAnswerText">Pre filled response from the QnA maker for  question by the user.</param>
        /// <param name="userTitleValue">Title description text.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string feedbackType,
            string personName,
            string personEmail,
            string incomingFeedbackText,
            string incomingQuestionText = "",
            string incomingAnswerText = "",
            string userTitleValue = "")
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

            return GetCardDetails(feedbackType, personName, personEmail, ref incomingFeedbackText, ref incomingQuestionText, ref incomingAnswerText, incomingSubtitleText, userTitleValue);
        }

        private static Attachment GetCardDetails(
                                                  string feedbackType,
                                                  string personName,
                                                  string personEmail,
                                                  ref string incomingFeedbackText,
                                                  ref string incomingQuestionText,
                                                  ref string incomingAnswerText,
                                                  string incomingSubtitleText,
                                                  string userTitleValue)
        {
            incomingQuestionText = string.IsNullOrEmpty(incomingQuestionText) ? "NA" : incomingQuestionText;
            incomingAnswerText = string.IsNullOrEmpty(incomingAnswerText) ? "NA" : incomingAnswerText;
            var chatTextButton = string.Format(Resource.ChatTextButton, personName);

            var variablesToValues = new Dictionary<string, string>()
            {
                { "titleText",  Resource.TitleText },
                { "userTitleValue", userTitleValue },
                { "descriptionText", Resource.DescriptionText },
                { "incomingFeedbackText", incomingFeedbackText },
                { "kbEntryText", Resource.KBEntryText },
                { "smeAnswer", incomingAnswerText },
                { "questionText", Resource.QuestionText },
                { "smeQuestion", incomingQuestionText },
                { "dateCreatedDisplayText", Resource.DateCreatedDisplayText },

                // TO-DO: need to pass date created value from the previous entity creation method
                { "dateCreatedValue",  DateTime.UtcNow.ToString() },
                { "incomingTitleText", feedbackType },
                { "incomingSubtitleText", incomingSubtitleText },
                { "personUpn", personEmail },
                { "chatTextButton", chatTextButton }
            };
            if (feedbackType == "Question For Expert")
            {
                variablesToValues.Add("statusShowCardButtonText", Resource.StatusShowCardButtonText);
                variablesToValues.Add("openStatusText", Resource.OpenStatusText);
                variablesToValues.Add("assignStatusText", Resource.AssignStatusText);
                variablesToValues.Add("closeStatusText", Resource.CloseStatusText);
                variablesToValues.Add("submitButtonText", Resource.SubmitButtonText);
                variablesToValues.Add("closedDispalyText", Resource.ClosedDispalyText);
                variablesToValues.Add("dateClosedValue", DateTime.UtcNow.ToString());
                variablesToValues.Add("statusText", Resource.StatusText);
                variablesToValues.Add("statusValue", Resource.OpenStatusText);
                return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
            }

            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "IncomingSMEFeedbackCard.json");
            var feedbackCardTemplate = File.ReadAllText(cardJsonFilePath);

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(feedbackCardTemplate, variablesToValues));
        }
    }
}