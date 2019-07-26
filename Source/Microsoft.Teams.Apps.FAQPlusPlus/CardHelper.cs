// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///  This is a common class that builds adaptive card attachment.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// This method constructs the Json replacing the values using Resx files.
        /// </summary>
        /// <param name="cardBody">Sends the Adaptive card body as Json String.</param>
        /// <param name="variablesToValues">.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static string GenerateCardBody(string cardBody, Dictionary<string, string> variablesToValues)
        {
            foreach (var kvp in variablesToValues)
            {
                cardBody = cardBody.Replace($"%{kvp.Key}%", kvp.Value);
            }

            return cardBody;
        }

        /// <summary>
        /// This method creates the card attachment using the Json.
        /// </summary>
        /// <param name="cardBody">Sends the Adaptive card body as Json String.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GenerateCardAttachment(string cardBody)
        {
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject<JObject>(cardBody),
            };
        }
    }
}