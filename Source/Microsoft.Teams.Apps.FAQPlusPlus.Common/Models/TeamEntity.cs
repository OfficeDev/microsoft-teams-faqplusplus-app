// <copyright file="TeamEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Team entity in storage.
    /// </summary>
    public class TeamEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamEntity"/> class.
        /// </summary>
        public TeamEntity()
        {
        }

        /// <summary>
        /// Gets or sets gets Team Id
        /// </summary>
        [JsonProperty("TeamId")]
        public string TeamId { get; set; }
    }
}
