﻿// <copyright file="UserActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    /// <summary>
    /// This model class is responsible to model user activity with bot-
    // asking a question or providing feedback on app or on results given by the bot to the user.
    // </summary>
    public class UserActivity
    {
        /// <summary>
        /// Gets or sets the bot feedback.
        /// </summary>
        public string AppFeedback { get; set; }

        /// <summary>
        /// Gets or sets the question for the expert being asked by the user through Response card-
        /// Response Card: Response generated by the bot to user question by calling QnA maker service.
        /// </summary>
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the question for the expert being asked by the user through bot command.
        /// </summary>
        public string QuestionForExpert { get; set; }

        /// <summary>
        /// Gets or sets the answer for the expert- Answer sent to the SME team along with feedback
        /// provided by the user on response given by bot calling QnA maker service.
        /// </summary>
        public string SmeAnswer { get; set; }

        /// <summary>
        /// Gets or sets the results feedback.
        /// </summary>
        public string ResultsFeedback { get; set; }

        /// <summary>
        /// Gets or sets the user title text for ask an expert button.
        /// </summary>
        public string QuestionUserTitleText { get; set; }

        /// <summary>
        /// Gets or sets the User title text for feedback button.
        /// </summary>
        public string FeedbackUserTitleText { get; set; }
    }
}
