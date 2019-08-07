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
        /// <param name="kbAnswer">Answer from the KB.</param>
        /// <returns>Constructed adaptive fact.</returns>
        public static string TruncateStringIfLonger(string kbAnswer)
        {
            return !string.IsNullOrWhiteSpace(kbAnswer) ? kbAnswer.Substring(0, KbAnswerMaxLength) + Ellipsis : Resource.NonApplicableString;
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
        /// Gets local time stamp for the user activity.
        /// </summary>
        /// <param name="ticketDescription">The current ticket information.</param>
        /// <returns>A description string.</returns>
        public static string GetDescriptionText(string ticketDescription)
        {
            return !string.IsNullOrWhiteSpace(ticketDescription) ? ticketDescription : Resource.NonApplicableString;
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