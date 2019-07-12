// <copyright file="GetKnowledgeBaseDetailsResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Get Knowledge Base Details Response
    /// </summary>
    public class GetKnowledgeBaseDetailsResponse
    {
        /// <summary>
        /// Gets or sets id
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }
    }
}
