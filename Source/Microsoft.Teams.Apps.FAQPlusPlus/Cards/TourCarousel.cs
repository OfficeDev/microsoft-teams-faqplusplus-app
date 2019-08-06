// <copyright file="TourCarousel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Tour Carousel feature : Common Method for user tour and team tour.
    /// </summary>
    public class TourCarousel
    {
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

        private static Attachment GetCard(string cardTitleText, string cardContentText, string carouselImage)
        {
            AdaptiveCard tourCarouselCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = cardTitleText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = cardContentText,
                        Wrap = true
                    },
                    new AdaptiveImage
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                        Url = new Uri(carouselImage),
                        Size = AdaptiveImageSize.Large
                    }
                }
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = tourCarouselCard,
            };
        }
    }
}
