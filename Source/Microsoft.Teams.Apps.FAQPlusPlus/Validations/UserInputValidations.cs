﻿// <copyright file="UserInputValidations.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.Validations
{
    using System.Threading;
    using Microsoft.Bot.Builder;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///  This Class Validates the User fields in Adaptive cards.
    /// </summary>
    public static class UserInputValidations
    {
        /// <summary>
        ///  Validates the User fields in Adaptive cards.
        /// </summary>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Return bool value.</returns>
        public static bool Validate(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var obj = JsonConvert.DeserializeObject<UserActivity>(turnContext.Activity.Value.ToString());
            obj.AppFeedback = obj.AppFeedback ?? string.Empty;
            obj.QuestionForExpert = obj.QuestionForExpert ?? string.Empty;
            obj.UserTitleText = obj.UserTitleText ?? string.Empty;
            obj.ResultsFeedback = obj.ResultsFeedback ?? string.Empty;

            if (obj.UserTitleText == string.Empty || obj.QuestionForExpert == string.Empty)
            {
                    turnContext.SendActivityAsync(MessageFactory.Text("All Fields are Mandatory"), cancellationToken);
                    return false;
            }

            return true;
        }
    }
}
