// <copyright file="MessagingExtensionConstants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// Class contains the constants to be used by Search service to identify the tab type received from message extension
    /// </summary>
    public class MessagingExtensionConstants
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