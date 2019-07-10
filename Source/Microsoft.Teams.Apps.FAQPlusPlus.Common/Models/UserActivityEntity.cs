// <copyright file="UserActivityEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Represents UserActivity entity used for storage and retrieval.
    /// </summary>
    public class UserActivityEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets created date which will be stored in table storage
        /// </summary>
        [JsonProperty("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets question which will be stored in table storage
        /// </summary>
        [JsonProperty("Question")]
        public string Question { get; set; }

        /// <summary>
        /// Gets request id which is stored in table storage
        /// </summary>
        public string RequestId
        {
            get
            {
                return this.RowKey;
            }
        }

        /// <summary>
        /// Gets or sets user email address which will be stored in table storage
        /// </summary>
        [JsonProperty("UserEmail")]
        public string UserEmail { get; set; }

        /// <summary>
        /// Gets or sets user first name which will be stored in table storage
        /// </summary>
        [JsonProperty("UserFirstName")]
        public string UserFirstName { get; set; }
    }
}
