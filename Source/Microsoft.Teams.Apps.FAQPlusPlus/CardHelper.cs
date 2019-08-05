// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///  This is a common class that builds adaptive card attachment.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// This method creates the card attachment using the Json.
        /// </summary>
        /// <param name="cardBody">Sends the adaptive card body as Json string.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GenerateCardAttachment(string cardBody)
        {
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject<JObject>(cardBody),
            };
        }

        /// <summary>
        /// Common method for constructing adaptivefact  for adaptive cards.
        /// </summary>
        /// <param name="title">Title for the fact.</param>
        /// <param name="value">Value for the fact.</param>
        /// <returns>Constructed adaptive fact.</returns>
        public static AdaptiveFact GetAdaptiveFact(string title, string value)
        {
            return new AdaptiveFact()
            {
                Title = title,
                Value = value
            };
        }

        /// <summary>
        /// Gets the closed date of the ticket.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>The closed date of the ticket.</returns>
        public static string GetTicketClosedDate(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Closed)
            {
                // We are using this format because DATE and TIME are not supported on mobile yet.
                return ticket.DateClosed.Value.ToLocalTime().ToString("ddd, MMM dd',' yyy hh':'mm tt");
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
                return string.IsNullOrEmpty(ticket.AssignedToName) ? "Open" : $"Assigned to {ticket.AssignedToName}";
            }
            else
            {
                return $"Closed by {ticket.LastModifiedByName}";
            }
        }

        /// <summary>
        /// Gets the user description text.
        /// </summary>
        /// <param name="ticketDescription">The current ticket information.</param>
        /// <returns>A description string.</returns>
        public static string GetDescriptionText(string ticketDescription)
        {
            return !string.IsNullOrWhiteSpace(ticketDescription) ? ticketDescription : Resource.NonApplicableString;
        }
    }
}