// <copyright file="UserInputValidations.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Validations
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This Class Validates the User fields in Adaptive cards.
    /// </summary>
    public static class UserInputValidations
    {
        /// <summary>
        ///  Validates the User fields in Adaptive cards.
        /// </summary>
        /// <param name="payload">The adaptive card payload.</param>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Return bool value.</returns>
        public static async Task<bool> Validate(SubmitUserRequestPayload payload, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            payload.AppFeedback = payload.AppFeedback ?? string.Empty;
            payload.QuestionForExpert = payload.QuestionForExpert ?? string.Empty;
            payload.QuestionUserTitleText = payload.QuestionUserTitleText ?? string.Empty;
            payload.FeedbackUserTitleText = payload.FeedbackUserTitleText ?? string.Empty;
            payload.ResultsFeedback = payload.ResultsFeedback ?? string.Empty;

            if (payload.QuestionUserTitleText == string.Empty && payload.FeedbackUserTitleText == string.Empty)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(Resource.MandatoryFieldText), cancellationToken);
                return false;
            }

            return true;
        }
    }
}
