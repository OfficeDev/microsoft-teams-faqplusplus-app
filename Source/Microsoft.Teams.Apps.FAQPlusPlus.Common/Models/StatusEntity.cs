// <copyright file="StatusEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Status entity used for storage and retrieval.
    /// </summary>
    public class StatusEntity : TableEntity
    {
        /// <summary>
        /// Gets status id which is stored in table storage
        /// </summary>
        public string StatusId
        {
            get
            {
                return this.RowKey;
            }
        }

        /// <summary>
        /// Gets or sets status value which will be stored in table storage
        /// </summary>
        [JsonProperty("StatusValue")]
        public string StatusValue { get; set; }
    }
}
