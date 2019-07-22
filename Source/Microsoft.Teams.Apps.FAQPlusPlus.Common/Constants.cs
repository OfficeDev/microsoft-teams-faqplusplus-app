// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// Class contains the constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Teams string to be used in Configuration Provider for identitying the entity type
        /// </summary>
        public const string TeamEntityType = "Teams";

        /// <summary>
        /// KnowledgeBase string to be used in Configuration Provider for identitying the entity type
        /// </summary>
        public const string KnowledgeBaseEntityType = "KnowledgeBase";

        /// <summary>
        /// WelcomeMessage string to be used in Configuration Provider for identitying the entity type
        /// </summary>
        public const string WelcomeMessageEntityType = "WelcomeMessage";

        /// <summary>
        /// StaticTab string to be used in Configuration Provider for identitying the entity type
        /// </summary>
        public const string StaticTabEntityType = "StaticTab";

        /// <summary>
        /// Ticket string to be used in Configuration Provider for identitying the entity type
        /// </summary>
        public const string TicketEntityType = "Ticket";

        /// <summary>
        /// Ticket status enum to be used to identify status available for ticket
        /// </summary>
        public enum TicketStatus
        {
            /// <summary>
            /// Close a ticket
            /// </summary>
            Closed = 0,

            /// <summary>
            /// Assign the ticket to SME
            /// </summary>
            Assign = 1,

            /// <summary>
            /// OnHold a ticket
            /// </summary>
            OnHold = 2
        }
    }
}
