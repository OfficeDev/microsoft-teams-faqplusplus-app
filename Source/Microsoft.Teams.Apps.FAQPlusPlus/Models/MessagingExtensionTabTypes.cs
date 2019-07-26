// <copyright file="MessagingExtensionTabTypes.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    /// <summary>
    /// Class contains the constants to be used by Search service to identify the tab type received from message extension
    /// </summary>
    public class MessagingExtensionTabTypes
    {
        /// <summary>
        /// Recent string to be used in Search service for identitying the tab type
        /// </summary>
        public const string RecentTabType = "Recent";

        /// <summary>
        /// Open string to be used in Search service for identitying the tab type
        /// </summary>
        public const string OpenTabType = "Open";

        /// <summary>
        /// Assigned string to be used in Search service for identitying the tab type
        /// </summary>
        public const string AssignedTabType = "Assigned";
    }
}
