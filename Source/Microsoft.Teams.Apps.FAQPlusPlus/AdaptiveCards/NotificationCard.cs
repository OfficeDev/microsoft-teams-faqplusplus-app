// <copyright file="NotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// The class process Confirmation card-upon bot sending the user query to Sme channel.
    /// </summary>
    public class NotificationCard
    {
        private const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";

        /// <summary>
        /// This method will construct the notification card that is sent to the user upon posting the request to Sme channel.
        /// </summary>
        /// <param name="question">Question asked by the user.</param>
        /// <param name="userTitleValue">Title text of user the activity.</param>
        /// <returns>The JSON string for the adaptive card.</returns>
        public static Attachment GetCard(string question, string userTitleValue)
        {
            question = string.IsNullOrEmpty(question) ? Resource.NotApplicable : question;
            var currentDateTime = DateTime.UtcNow.ToString(DateFormat);

            AdaptiveCard userNotificationCard = new AdaptiveCard("1.0");
            userNotificationCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.NotificationCardTitleText,
                Wrap = true
            });

            userNotificationCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.NotificationAdaptiveCardContent,
                Spacing = AdaptiveSpacing.Small,
                Wrap = true
            });

            var factSetList = GetFactSetList(userTitleValue, question, currentDateTime);
            userNotificationCard.Body.Add(new AdaptiveFactSet() { Facts = factSetList });
            return CardHelper.GenerateCardAttachment(userNotificationCard.ToJson());
        }

        private static List<AdaptiveFact> GetFactSetList(string userTitleValue, string incomingQuestionText, string currentDateTime)
        {
            return new List<AdaptiveFact>()
                {
                    CardHelper.GetAdaptiveFact(Resource.StatusFactTitle, Resource.OpenStatusValue),
                    CardHelper.GetAdaptiveFact(Resource.TitleText, userTitleValue),
                    CardHelper.GetAdaptiveFact(Resource.DescriptionText, incomingQuestionText),
                    CardHelper.GetAdaptiveFact(Resource.DateCreatedDisplayFactTitle, "{{DATE(" + currentDateTime + ", SHORT)}} at {{TIME(" + currentDateTime + ")}}"),
                    CardHelper.GetAdaptiveFact(Resource.ClosedFactTitle, Resource.NotApplicable),
                };
        }
    }
}