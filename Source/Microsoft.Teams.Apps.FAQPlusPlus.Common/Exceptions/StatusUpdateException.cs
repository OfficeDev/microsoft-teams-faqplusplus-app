// <copyright file="StatusUpdateException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions
{
    using System;

    /// <summary>
    /// The exception class for the status updates.
    /// </summary>
    public class StatusUpdateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusUpdateException"/> class.
        /// </summary>
        public StatusUpdateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusUpdateException"/> class.
        /// </summary>
        /// <param name="message">The message to capture.</param>
        public StatusUpdateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusUpdateException"/> class.
        /// </summary>
        /// <param name="message">The message to capture/throw.</param>
        /// <param name="inner">The inner exception.</param>
        public StatusUpdateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}