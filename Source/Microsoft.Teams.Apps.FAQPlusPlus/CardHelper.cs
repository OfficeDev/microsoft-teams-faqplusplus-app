// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods
{
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///  This is a common class that builds adaptive card attachment.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// This method creates the card attachment using the Json.
        /// </summary>
        /// <param name="cardBody">Sends the adaptive card body as Json string.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GenerateCardAttachment(string cardBody)
        {
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject<JObject>(cardBody),
            };
        }

        public static AdaptiveFact GetAdaptiveFact(string title, string value)
        {
            return new AdaptiveFact()
            {
                Title = title,
                Value = value
            };
        }

        public static AdaptiveChoice GetChoiceSet(string title, string value)
        {
            return new AdaptiveChoice()
            {
                Title = title,
                Value = value
            };
        }
    }
}