// <copyright file="ChangeTicketStatusPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data payload of Action.Submit to change the status of a ticket.
    /// </summary>
    public class ChangeTicketStatusPayload
    {
        /// <summary>
        /// Action that reopens a closed ticket
        /// </summary>
        public const string ReopenAction = "0";

        /// <summary>
        /// Action that closes a ticket
        /// </summary>
        public const string CloseAction = "1";

        /// <summary>
        /// Action that assigns a ticket to the person that performed the action
        /// </summary>
        public const string AssignToSelfAction = "2";

        /// <summary>
        /// Gets or sets the ticket id.
        /// </summary>
        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        [JsonProperty("statuscode")]
        public string Status { get; set; }
    }
}