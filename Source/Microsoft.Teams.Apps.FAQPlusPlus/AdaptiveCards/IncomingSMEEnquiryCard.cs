// <copyright file="IncomingSMEEnquiryCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
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
        private const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
        private const string Ellipsis = "...";

        /// <summary>
        /// Create a card that represents application feedback.
        /// </summary>
        /// <param name="incomingTitleValue">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccountDetails">Details of the user submitting the feedback.</param>
        /// <param name="userActivityPayload">Payload from the feedback submission.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateAppFeedbackCard(
            string incomingTitleValue,
            TeamsChannelAccount userAccountDetails,
            UserActivity userActivityPayload)
        {
            var incomingSubtitleText = string.Format(Resource.IncomingFeedbackSubHeaderText, userAccountDetails.Name, Resource.AppFeedbackText);
            return GetCard(Resource.AppFeedbackText, incomingTitleValue, incomingSubtitleText, userAccountDetails, userActivityPayload);
        }

        /// <summary>
        /// Create a card that represents result feedback.
        /// </summary>
        /// <param name="incomingTitleValue">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccountDetails">Details of the user submitting the feedback.</param>
        /// <param name="userActivityPayload">Payload from the feedback submission.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateResultFeedbackCard(
            string incomingTitleValue,
            TeamsChannelAccount userAccountDetails,
            UserActivity userActivityPayload)
        {
            var incomingSubtitleText = string.Format(Resource.IncomingFeedbackSubHeaderText, userAccountDetails.Name, Resource.ResultsFeedbackText);
            return GetCard(Resource.ResultsFeedbackText, incomingTitleValue, incomingSubtitleText, userAccountDetails, userActivityPayload);
        }

        /// <summary>
        /// Create a card that represents a ticket.
        /// </summary>
        /// <param name="incomingTitleValue">Actual title text entered by the user for the given scenario.</param>
        /// <param name="userAccountDetails">Details of the user submitting the ticket.</param>
        /// <param name="userActivityPayload">Payload from the ticket submission.</param>
        /// <returns>The card as an attachment</returns>
        public static Attachment CreateTicketCard(
            string incomingTitleValue,
            TeamsChannelAccount userAccountDetails,
            UserActivity userActivityPayload)
        {
            var incomingSubtitleText = string.Format(Resource.QuestionForExpertSubHeaderText, userAccountDetails.Name, Resource.QuestionForExpertText);
            return GetCard(Resource.QuestionForExpertText, incomingTitleValue, incomingSubtitleText, userAccountDetails, userActivityPayload, true);
        }

        /// <summary>
        /// This method will construct the adaptive card that is sent to the Sme team.
        /// </summary>
        /// <param name="incomingTitleText">Title of the user activity-for feedback or ask an expert.</param>
        /// <param name="incomingTitleValue">Actual title text entered by the user for the given scenario.</param>
        /// <param name="incomingSubtitleText">Adaptive card subtitle text based on the user activity type.</param>
        /// <param name="channelAccountDetails">Channel details to which bot post the user question.</param>
        /// <param name="userActivityPayload">User activity type:posting a feedback or asking a question to the expert.</param>
        /// <param name="isStatusAvailable">Flag value for status button- required only for ask an expert scenarios.</param>
        /// <returns>The card JSON string.</returns>
        public static Attachment GetCard(
            string incomingTitleText,
            string incomingTitleValue,
            string incomingSubtitleText,
            TeamsChannelAccount channelAccountDetails,
            UserActivity userActivityPayload,
            bool isStatusAvailable = false)
        {
            var incomingAnswerText = string.IsNullOrEmpty(userActivityPayload.SmeAnswer) ? Resource.NotApplicable : userActivityPayload.SmeAnswer;
            var userQuestion = string.IsNullOrEmpty(userActivityPayload.UserQuestion) ? Resource.NotApplicable : userActivityPayload.UserQuestion;
            var chatTextButton = string.Format(Resource.ChatTextButton, channelAccountDetails.GivenName);
            if (incomingAnswerText.Length > 500)
            {
                incomingAnswerText = incomingAnswerText.Substring(0, 500) + Ellipsis;
            }

            var currentDateTime = DateTime.UtcNow.ToString(DateFormat);

            // Constructing adaptive card that is sent to Sme team.
            AdaptiveCard incomingSmeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = incomingTitleText,
                        Color = AdaptiveTextColor.Attention,
                        Size = AdaptiveTextSize.Medium
                    },
                    new AdaptiveTextBlock
                    {
                        Text = incomingSubtitleText,
                        Wrap = true
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

            // Calling GetFactSetList.
            var factSetList = GetFactSetList(incomingTitleValue, GetQuestionText(userActivityPayload), incomingAnswerText, userQuestion, currentDateTime);

            // Sme card which has both change status button and chat with person button.
            if (isStatusAvailable)
            {
                factSetList.Add(CardHelper.GetAdaptiveFact(Resource.ClosedFactTitle, Resource.NotApplicable));

                AdaptiveShowCardAction showCard = new AdaptiveShowCardAction
                {
                    Card = new AdaptiveCard("1.0")
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveChoiceSetInput
                            {
                                Id = "statuscode",
                                Style = AdaptiveChoiceInputStyle.Compact,
                                IsMultiSelect = false,
                                Value = "1",
                                Choices = GetChoiceSetList()
                            }
                        },
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Title = Resource.SubmitButtonText
                            }
                        }
                    }
                };
                incomingSmeCard.Actions.Add(new AdaptiveShowCardAction()
                {
                    Title = Resource.ChangeStatusButtonText,
                    Card = showCard.Card
                });
                incomingSmeCard.Body.Add(new AdaptiveFactSet() { Facts = factSetList });
                return CardHelper.GenerateCardAttachment(incomingSmeCard.ToJson());
            }

            incomingSmeCard.Body.Add(new AdaptiveFactSet() { Facts = factSetList });
            return CardHelper.GenerateCardAttachment(incomingSmeCard.ToJson());
        }

        // Method that returns the Choice set used for change status show card on Sme notification card.
        private static List<AdaptiveChoice> GetChoiceSetList()
        {
            return new List<AdaptiveChoice>()
                   {
                        CardHelper.GetChoiceSet(Resource.AssignStatusText, "1"),
                       CardHelper.GetChoiceSet(Resource.CloseStatusText, "2")
                   };
        }

        // Method that returns the Fact set used in Sme notification card.
        private static List<AdaptiveFact> GetFactSetList(string incomingTitleValue, string incomingQuestionText, string incomingAnswerText, string userQuestion, string currentDateTime)
        {
            return new List<AdaptiveFact>()
                {
                    CardHelper.GetAdaptiveFact(Resource.StatusFactTitle, Resource.OpenStatusValue),
                    CardHelper.GetAdaptiveFact(Resource.TitleText, incomingTitleValue),
                    CardHelper.GetAdaptiveFact(Resource.DescriptionText, incomingQuestionText),
                    CardHelper.GetAdaptiveFact(Resource.KBEntryText, incomingAnswerText),
                    CardHelper.GetAdaptiveFact(Resource.QuestionText, userQuestion),
                    CardHelper.GetAdaptiveFact(Resource.DateCreatedDisplayFactTitle, "{{DATE(" + currentDateTime + ", SHORT)}} at {{TIME(" + currentDateTime + ")}}"),
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