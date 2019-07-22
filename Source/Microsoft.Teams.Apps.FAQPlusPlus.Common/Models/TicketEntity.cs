// <copyright file="TicketEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Ticket entity used for storage and retrieval.
    /// </summary>
    public class TicketEntity : TableEntity
    {
        /// <summary>
        /// Gets unique ticket Id which is stored in table storage
        /// </summary>
        [JsonProperty("TicketId")]
        public string TicketId
        {
            get
            {
                return this.RowKey;
            }
        }

        /// <summary>
        /// Gets or sets comments as text about the ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("Text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets ticket opened by SME updating the ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("OpenedBy")]
        public string OpenedBy { get; set; }

        /// <summary>
        /// Gets or sets status of the ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("Status")]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets assigned SME currently working on the ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("AssignedTo")]
        public string AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets created date of ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("DateCreated")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets assigned date of ticket which will be stored in table storage
        /// </summary>
        [JsonProperty("DateAssigned")]
        public DateTime DateAssigned { get; set; }

        /// <summary>
        /// Gets or sets opened by conversation Id of 1:1 chat which will be stored in table storage
        /// </summary>
        [JsonProperty("OpenedByConversationId")]
        public string OpenedByConversationId { get; set; }

        /// <summary>
        /// Gets or sets thread conversation Id of conversation thread which will be stored in table storage
        /// </summary>
        [JsonProperty("ThreadConversationId")]
        public string ThreadConversationId { get; set; }

        /// <summary>
        /// Gets or sets card activity Id which will be stored in table storage
        /// </summary>
        [JsonProperty("CardActivityId")]
        public string CardActivityId { get; set; }
    }
}