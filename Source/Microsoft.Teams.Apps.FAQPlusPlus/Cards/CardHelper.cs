// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Utility functions for constructing cards used in this project.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// Maximum length of the knowledge base answer to show.
        /// </summary>
        public const int KbAnswerMaxLength = 500;

        /// <summary>
        /// Maximum length of the description text to show.
        /// </summary>
        public const int DescriptionText = 200;

        /// <summary>
        /// Maximum length of title text to show.
        /// </summary>
        public const int Title = 50;

        private const string Ellipsis = "...";

        /// <summary>
        /// Truncate the provided string to a given maximum length.
        /// </summary>
        /// <param name="text">Text to be truncated.</param>
        /// <param name="maxLength">The maximum length in characters of the text.</param>
        /// <returns>Truncated string.</returns>
        public static string TruncateStringIfLonger(string text, int maxLength)
        {
            if ((text != null) && (text.Length > maxLength))
            {
                text = text.Substring(0, maxLength) + Ellipsis;
            }

            return text;
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of the user activity.</param>
        /// <returns>The closed date of the ticket.</returns>
        public static string GetTicketClosedDate(TicketEntity ticket, DateTimeOffset? activityLocalTimestamp)
        {
            if (ticket.Status == (int)TicketState.Closed)
            {
                // We are using this format because DATE and TIME are not supported on mobile yet.
                return GetFormattedDateInUserTimeZone(ticket.DateClosed.Value, activityLocalTimestamp);
            }
            else
            {
                return Resource.NonApplicableString;
            }
        }

        /// <summary>
        /// Gets the ticket status currently.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A status string.</returns>
        public static string GetTicketStatus(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Open)
            {
                return string.IsNullOrEmpty(ticket.AssignedToName) ? Resource.OpenStatusTitle : string.Format(Resource.AssignedToStatusValue, ticket.AssignedToName);
            }
            else
            {
                return string.Format(Resource.ClosedByStatusValue, ticket.LastModifiedByName);
            }
        }

        /// <summary>
        /// Return "N/A" if the given text is null or empty, or the text unchanged, otherwise.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>A string or N/A.</returns>
        public static string ConvertNullOrEmptyToNotApplicable(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value : Resource.NonApplicableString;
        }

        /// <summary>
        /// Returns a string that will display the given date and time in the user's local time zone, when placed in an adaptive card.
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <param name="userLocalTime">The sender's local time, as determined by the local timestamp of the activity.</param>
        /// <returns>A description string.</returns>
        public static string GetFormattedDateInUserTimeZone(DateTime dateTime, DateTimeOffset? userLocalTime)
        {
            // Adaptive card on mobile has a bug where it does not support DATE and TIME, so for now we convert the date and time manually
            // TODO: Change to use DATE() function
            return dateTime.Add(userLocalTime?.Offset ?? TimeSpan.FromMinutes(0)).ToShortDateString();
        }
    }
}