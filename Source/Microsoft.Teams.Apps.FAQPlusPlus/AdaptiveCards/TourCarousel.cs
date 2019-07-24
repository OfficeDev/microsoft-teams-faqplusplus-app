// <copyright file="TourCarousel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods;

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
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="cardTitleText">Card Title Text.</param>
        /// <param name="cardContentText">Title of the Card.</param>
        /// <param name="carouselImage">Image for the Card.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string cardTitleText, string cardContentText, string carouselImage)
        {
            var variablesToValues = new Dictionary<string, string>()
            {
                { "cardTitleText", cardTitleText },
                { "cardContentText", cardContentText },
                { "carouselImage", carouselImage },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
