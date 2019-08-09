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
        ///  Validates the user fields in cards.
        /// </summary>
        /// <param name="payload">The adaptive card payload.</param>
        /// <param name="descriptionText">Description entered by user.</param>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Return bool value.</returns>
        public static async Task<bool> Validate(SubmitUserRequestPayload payload, string descriptionText, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            payload.AppFeedback = payload.AppFeedback ?? string.Empty;
            payload.QuestionForExpert = payload.QuestionForExpert ?? string.Empty;
            payload.QuestionUserTitleText = payload.QuestionUserTitleText ?? string.Empty;
            payload.FeedbackRatingAction = payload.FeedbackRatingAction ?? string.Empty;
            payload.UserQuestion = payload.UserQuestion ?? string.Empty;

            var userDescriptionText = payload.QuestionForExpert == string.Empty ? payload.UserQuestion : payload.QuestionForExpert;
            if (string.IsNullOrWhiteSpace(payload.QuestionUserTitleText) && descriptionText == SubmitUserRequestPayload.QuestionForExpertAction)
            {
                await turnContext.UpdateActivityAsync(GetCardActivity(turnContext, AskAnExpertCard.GetCard(true, userDescriptionText), cancellationToken), cancellationToken);
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.FeedbackRatingAction) && descriptionText == SubmitUserRequestPayload.AppFeedbackAction)
            {
                await turnContext.UpdateActivityAsync(GetCardActivity(turnContext, ShareFeedbackCard.GetCard(true, userDescriptionText), cancellationToken), cancellationToken);
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