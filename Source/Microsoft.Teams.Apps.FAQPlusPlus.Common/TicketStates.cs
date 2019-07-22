// <copyright file="TicketStates.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// Class contains the ticket states
    /// </summary>
    public class TicketStates
    {
        /// <summary>
        /// Ticket state enum to be used to identify states available for ticket
        /// </summary>
        public enum TicketState
        {
            /// <summary>
            /// Close a ticket
            /// </summary>
            Closed = 0,

            /// <summary>
            /// Open a already closed ticket
            /// </summary>
            Open = 1,

            /// <summary>
            /// Assign the ticket to SME
            /// </summary>
            Assigned = 2,
        }
    }
}
