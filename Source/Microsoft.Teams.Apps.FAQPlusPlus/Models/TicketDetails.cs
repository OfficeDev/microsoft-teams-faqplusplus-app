// <copyright file="TicketDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// The class that models the ticket details.
    /// </summary>
    public class TicketDetails
    {
        /// <summary>
        /// Gets or sets the RowKey.
        /// </summary>
        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        [JsonProperty("statuscode")]
        public string Status { get; set; }
    }
}