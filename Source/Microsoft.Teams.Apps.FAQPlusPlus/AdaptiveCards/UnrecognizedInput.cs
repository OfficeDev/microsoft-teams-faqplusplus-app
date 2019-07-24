// <copyright file="UnrecognizedInput.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the user-asking random question to bot.
    /// </summary>
    public static class UnrecognizedInput
    {
        private static readonly string CardTemplate;

        static UnrecognizedInput()
        {
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "UnrecognizedInput.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="question">The question that the user asks the bot.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string question)
        {
            var questionLineText = string.Format(Resource.QuestionLineText, question);
            var customMessage = Resource.CustomMessage;
            var askAnExpertButtonText = Resource.AskAnExpertButtonText;
            var variablesToValues = new Dictionary<string, string>()
            {
                { "questionLineText", questionLineText },
                { "customMessage", customMessage },
                { "askAnExpertButtonText", askAnExpertButtonText },
                { "resultQuestionText", question },
                { "submitButtonText",  Resource.SubmitButtonText },
            };

            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}
