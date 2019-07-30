// <copyright file="TicketEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Search;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Ticket entity used for storage and retrieval.
    /// </summary>
    public class TicketEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets unique ticket Id which is stored in table storage
        /// </summary>
        [Key]
        [JsonProperty("TicketId")]
        public string TicketId { get; set; }

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
        /// Gets or sets the email address of the person that has opened the request originally
        /// </summary>
        [JsonProperty("OpenedByUpn")]
        public string OpenedByUpn { get; set; }

        /// <summary>
        /// Gets or sets the Aad Object Id of the person that has opened the request
        /// </summary>
        [JsonProperty("OpenedByObjectId")]
        public string OpenedByObjectId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the person that has opened the request
        /// </summary>
        [JsonProperty("OpenedByFirstName")]
        public string OpenedByFirstName { get; set; }

        /// <summary>
        /// Gets or sets status of the ticket which will be stored in table storage
        /// </summary>
        [IsSortable]
        [IsFilterable]
        [JsonProperty("Status")]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets assigned SME currently working on the ticket which will be stored in table storage
        /// </summary>
        [IsSearchable]
        [IsFilterable]
        [JsonProperty("AssignedTo")]
        public string AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets created date of ticket which will be stored in table storage
        /// </summary>
        [IsSortable]
        [JsonProperty("DateCreated")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets assigned date of ticket which will be stored in table storage
        /// </summary>
        [IsSortable]
        [JsonProperty("DateAssigned")]
        public DateTime? DateAssigned { get; set; }

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

        /// <summary>
        /// Gets or sets the AadObjectId of the SME that self-assigns a ticket
        /// </summary>
        [JsonProperty("AssignedToObjectId")]
        public string AssignedToObjectId { get; set; }

        /// <summary>
        /// Gets or sets the user title text
        /// </summary>
        [JsonProperty("UserTitleText")]
        public string UserTitleText { get; set; }

        /// <summary>
        /// Gets or sets the question that has been stored in the knowledge base
        /// </summary>
        [JsonProperty("KbEntryQuestion")]
        public string KbEntryQuestion { get; set; }

        /// <summary>
        /// Gets or sets the response that has been stored in the knowledge base
        /// </summary>
        [JsonProperty("KbEntryResponse")]
        public string KbEntryResponse { get; set; }
    }
}