// <copyright file="KnowledgeBaseEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents knowledge base entity used for retrieval.
    /// </summary>
    public class KnowledgeBaseEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets gets knowledge base Id
        /// </summary>
        [JsonProperty("KnowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }
    }
}
