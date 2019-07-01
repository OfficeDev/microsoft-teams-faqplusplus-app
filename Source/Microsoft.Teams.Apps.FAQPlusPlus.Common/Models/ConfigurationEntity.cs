// <copyright file="ConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents configuration entity used for storage and retrieval.
    /// </summary>
    public class ConfigurationEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets gets team Id
        /// </summary>
        [JsonProperty("TeamId")]
        public string TeamId { get; set; }
    }
}
