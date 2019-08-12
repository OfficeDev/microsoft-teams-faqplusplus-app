// <copyright file="UserInputValidations.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Validations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;

    /// <summary>
    /// Validation functions used for processing the user entered data in the mandatory UI fields.
    /// </summary>
    public static class UserInputValidations
    {
        /// <summary>
        ///  Validates the user on submit action for ask an expert scenario.
        /// </summary>
        /// <param name="payload">The adaptive card payload.</param>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Return bool value.</returns>
        public static async Task<bool> ValidateQuestionForExpert(SubmitUserRequestPayload payload, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            payload.QuestionUserTitleText = payload.QuestionUserTitleText ?? string.Empty;

            if (string.IsNullOrWhiteSpace(payload.QuestionUserTitleText))
            {
                await turnContext.UpdateActivityAsync(GetCardActivity(turnContext, AskAnExpertCard.GetCard(true, payload.UserQuestion, payload.QuestionForExpert, payload.SmeAnswer), cancellationToken), cancellationToken);
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Validates the user on submit action for share feedback scenario.
        /// </summary>
        /// <param name="payload">The adaptive card payload.</param>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Return bool value.</returns>
        public static async Task<bool> ValidateFeedback(SubmitUserRequestPayload payload, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            payload.FeedbackRatingAction = payload.FeedbackRatingAction ?? string.Empty;

            if (string.IsNullOrWhiteSpace(payload.FeedbackRatingAction))
            {
                await turnContext.UpdateActivityAsync(GetCardActivity(turnContext, ShareFeedbackCard.GetCard(true, payload.UserQuestion, payload.QuestionForExpert, payload.SmeAnswer), cancellationToken), cancellationToken);
                return false;
            }

            return true;
        }

        private static Activity GetCardActivity(ITurnContext turnContext, Attachment attachment, CancellationToken cancellationToken)
        {
            return new Activity(ActivityTypes.Message)
            {
                Id = turnContext.Activity.ReplyToId,
                Conversation = new ConversationAccount { Id = turnContext.Activity.Conversation.Id },
                Attachments = new List<Attachment> { attachment },
            };
        }
    }
}