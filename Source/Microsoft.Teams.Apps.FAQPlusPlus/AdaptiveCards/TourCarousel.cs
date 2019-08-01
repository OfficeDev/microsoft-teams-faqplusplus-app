// <copyright file="TourCarousel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Tour Carousel feature : Common Method for user tour and team tour.
    /// </summary>
    public class TourCarousel
    {
        private static readonly string CardTemplate;

        static TourCarousel()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "TourCarousel.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// Create the set of cards that comprise the team tour carousel.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted</param>
        /// <returns>The cards that comprise the team tour</returns>
        public static IEnumerable<Attachment> GetTeamTourCards(string appBaseUri)
        {
            return new List<Attachment>()
            {
                GetCard(Resource.TeamFunctionCardHeaderText, Resource.TeamFunctionCardContent, appBaseUri + "/content/Alert.png"),
                GetCard(Resource.TeamChatHeaderText, Resource.TeamChatCardContent, appBaseUri + "/content/Userchat.png"),
                GetCard(Resource.TeamQueryHeaderText, Resource.TeamQueryCardContent, appBaseUri + "/content/Ticket.png"),
            };
        }

        /// <summary>
        /// Create the set of cards that comprise the user tour carousel.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted</param>
        /// <returns>The cards that comprise the user tour</returns>
        public static IEnumerable<Attachment> GetUserTourCards(string appBaseUri)
        {
            return new List<Attachment>()
            {
                GetCard(Resource.FunctionCardText1, Resource.FunctionCardText2, appBaseUri + "/content/Qnamaker.png"),
                GetCard(Resource.AskAnExpertText1, Resource.AskAnExpertText2, appBaseUri + "/content/Askanexpert.png"),
                GetCard(Resource.ShareFeedbackTitleText, Resource.FeedbackText1, appBaseUri + "/content/Shareappfeedback.png"),
            };
        }

        private static Attachment GetCard(string title, string text, string imageUri)
        {
            var variablesToValues = new Dictionary<string, string>()
            {
                { "cardTitleText", title },
                { "cardContentText", text },
                { "carouselImage", imageUri },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
