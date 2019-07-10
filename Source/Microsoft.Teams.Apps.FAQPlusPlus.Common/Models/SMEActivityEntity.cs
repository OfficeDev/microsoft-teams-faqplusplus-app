// <copyright file="SMEActivityEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents SMEActivity entity used for storage and retrieval.
    /// </summary>
    public class SMEActivityEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets created by which will be stored in table storage
        /// </summary>
        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets created date which will be stored in table storage
        /// </summary>
        [JsonProperty("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets request Id which is stored in table storage
        /// </summary>
        public string RequestId
        {
            get
            {
                return this.RowKey;
            }
        }

        /// <summary>
        /// Gets or sets status Id which will be stored in table storage
        /// </summary>
        [JsonProperty("StatusId")]
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets update by which will be stored in table storage
        /// </summary>
        [JsonProperty("UpdatedBy")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets updated date which will be stored in table storage
        /// </summary>
        [JsonProperty("UpdatedDate")]
        public DateTime UpdatedDate { get; set; }
    }
}