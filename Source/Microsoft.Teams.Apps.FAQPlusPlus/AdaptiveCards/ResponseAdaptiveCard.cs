// <copyright file="ResponseAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseAdaptiveCard
    {
        private static readonly string CardTemplate;

        static ResponseAdaptiveCard()
        {
            var cardJsonFilePath = Path.Combine(".",  "AdaptiveCards", "ResponseAdaptiveCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="question">The question that the user asks the bot.</param>
        /// <param name="answer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string question, string answer)
        {
            var questionLineText = string.Format(Resource.QuestionLineText, question);
            var answerLineText = string.Format(Resource.AnswerLineText, answer);

            var variablesToValues = new Dictionary<string, string>()
            {
               { "questionLineText", questionLineText },
               { "answerLineText", answerLineText },
               { "resultQuestionText", question },
               { "resultAnswerText", answer },
               { "askAnExpertButtonText",  Resource.AskAnExpertButtonText },
               { "submitButtonText",  Resource.SubmitButtonText },
               { "shareResultsFeedbackButtonText", Resource.ShareFeedbackTitleText },
               { "resultsFeedbackDetails", Resource.Resultsfeedbackdetails },
            };
            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}