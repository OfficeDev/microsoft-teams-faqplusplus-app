// <copyright file="ResponseAdaptiveCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
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
            var cardJsonFilePath = Path.Combine(".", "AdaptiveCards", "ResponseAdaptiveCard.json");
            CardTemplate = File.ReadAllText(cardJsonFilePath);
        }

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <param name="question">Actual question from the QnA maker service.</param>
        /// <param name="answer">The response that the bot retrieves after querying the knowledge base.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string question, string answer, string userQuestion)
        {
            var variablesToValues = new Dictionary<string, string>()
            {
               { "responseHeaderText", Resource.ResponseHeaderText },
               { "questionLineText", question },
               { "userQuestionText", userQuestion },
               { "answerLineText", answer },
               { "askAnExpertButtonText",  Resource.AskAnExpertButtonText },
               { "askAnExpertDisplayText", Resource.AskAnExpertDisplayText },
               { "titleText", Resource.TitleText },
               { "mandatoryFieldText", Resource.MandatoryFieldText },
               { "showCardTitleText", Resource.ShowCardTitleText },
               { "descriptionText", Resource.DescriptionText },
               { "resultQuestionText", question },
               { "submitButtonText",  Resource.SubmitButtonText },
               { "shareResultsFeedbackButtonText", Resource.ShareFeedbackButtonText },
               { "shareFeedbackDisplayText", Resource.ShareFeedbackDisplayText },
               { "resultsFeedbackDetails", Resource.Resultsfeedbackdetails },
            };
            return CardHelper.GenerateCardAttachment(CardHelper.GenerateCardBody(CardTemplate, variablesToValues));
        }
    }
}