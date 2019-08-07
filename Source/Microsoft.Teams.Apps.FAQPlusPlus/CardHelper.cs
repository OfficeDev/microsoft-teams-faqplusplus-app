// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This is a card helper class used for  repetitive functions.
    /// </summary>
    public static class CardHelper
    {
        public const int KbAnswerMaxLength = 500;
        private const string Ellipsis = "...";
        private const string DateFormat = "ddd, MMM dd',' yyy hh':'mm tt";

        /// <summary>
        /// Gets the shortened Kb answer limited 500 characters.
        /// </summary>
        /// <param name="text">Text to be truncated.</param>
        /// <param name="maxLength">Text gets truncated by defined max length.</param>
        /// <returns>Constructed adaptive fact.</returns>
        public static string TruncateStringIfLonger(string text, int maxLength)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Length > maxLength)
                {
                    return text.Substring(0, maxLength) + Ellipsis;
                }

                return text;
            }
            else
            {
                return Resource.NonApplicableString;
            }
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <param name="localTimeStamp">Local time stamp of the user activity.</param>
        /// <returns>The closed date of the ticket.</returns>
        public static string GetTicketClosedDate(TicketEntity ticket, DateTimeOffset? localTimeStamp)
        {
            if (ticket.Status == (int)TicketState.Closed)
            {
                // We are using this format because DATE and TIME are not supported on mobile yet.
                return GetLocalTimeStamp(localTimeStamp);
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
        /// Common method to check the string value if it is null or empty.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>A string or N/A.</returns>
        public static string ValidateTextIsNullorEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value : Resource.NonApplicableString;
        }

        /// <summary>
        /// Gets the user description text.
        /// </summary>
        /// <param name="localTimeStamp">The current ticket information.</param>
        /// <returns>A description string.</returns>
        public static string GetLocalTimeStamp(DateTimeOffset? localTimeStamp)
        {
            return localTimeStamp.Value.ToString(DateFormat);
        }
    }
}