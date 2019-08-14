// <copyright file="TeamsAdaptiveCardSubmitActionData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines Teams-specific behavior for an adaptive card submit action.
    /// </summary>
    public class TeamsAdaptiveCardSubmitActionData
    {
        /// <summary>
        /// Gets or sets the Teams-specific action
        /// </summary>
        [JsonProperty("msteams")]
        public CardAction MsTeams { get; set; }
    }
}