// <copyright file="TourCarousel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods;
    
    /// <summary>
    ///  This class process Tour Carousel feature : Common Method for user tour and team tour.
    /// </summary>
    public class TourCarousel
    {
        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="cardTitleText">Card Title Text.</param>
        /// <param name="cardContentText">Title of the Card.</param>
        /// <param name="carouselImage">Image for the Card.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string cardTitleText, string cardContentText, string carouselImage)
        {
            AdaptiveCard tourCarouselCard = new AdaptiveCard("1.0");
            tourCarouselCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = cardTitleText,
                Wrap = true
            });

            tourCarouselCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = cardContentText,
                Wrap = true
            });

            tourCarouselCard.Body.Add(new AdaptiveImage()
            {
               HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Url = new Uri(carouselImage),
               Size = AdaptiveImageSize.Large
            });

            return CardHelper.GenerateCardAttachment(tourCarouselCard.ToJson());
        }
    }
}
