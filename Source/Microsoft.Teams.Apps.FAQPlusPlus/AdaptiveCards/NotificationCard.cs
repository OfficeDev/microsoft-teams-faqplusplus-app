// <copyright file="NotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// The class process Confirmation card-upon bot sending the user query to SME channel.
    /// </summary>
    public class NotificationCard
    {
        private static readonly string CardTemplate;

        /// <summary>
        /// Initializes static members of the <see cref="NotificationCard"/> class.
        /// </summary>
        static NotificationCard()
        {
            var cardJsonFilePath = Path.Combine(".",  "AdaptiveCards", "NotificationCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="question">Question asked by the user.</param>
        /// <param name="userTitleValue">Title text of user the activity.</param>
        /// <returns>The JSON string for the adaptive card.</returns>
        public static Attachment GetCard(string question, string userTitleValue)
        {
            question = string.IsNullOrEmpty(question) ? "NA" : question;
            var variablesToValues = new Dictionary<string, string>()
            {
                { "notificationCardTitleText", Resource.NotificationCardTitleText },
                { "notificationAdaptiveCardContent", Resource.NotificationAdaptiveCardContent },
                { "dateCreatedDisplayFactTitle", Resource.DateCreatedDisplayFactTitle },

                // TO-DO: need to pass date created value from the previous entity creation method
                { "dateCreatedValue",  DateTime.Now.ToString("s") + "Z" },
                { "closedDisplayFactTitle",  Resource.ClosedFactTitle },
                { "statusText",  Resource.StatusFactTitle },

                 // TO-DO: need to pass dynamic status change as per the updated conversation
                { "statusValue",  Resource.OpenStatusText },
                { "titleText",  Resource.TitleText },
                { "userTitleValue", userTitleValue },
                { "descriptionText", Resource.DescriptionText },
                { "smeQuestion", question },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}