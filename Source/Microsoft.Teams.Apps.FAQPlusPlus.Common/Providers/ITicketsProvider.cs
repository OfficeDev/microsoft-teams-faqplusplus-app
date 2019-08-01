// <copyright file="ITicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Tickets provider
    /// </summary>
    public interface ITicketsProvider
    {
        /// <summary>
        /// Save or update ticket entity.
        /// </summary>
        /// <param name="ticketEntity">ticketEntity received from bot based on which appropriate row will replaced or inserted in table storage</param>
        /// <returns><see cref="Task"/> that resolves successfully if the data was saved successfully.</returns>
        Task SaveOrUpdateTicketEntityAsync(TicketEntity ticketEntity);

        /// <summary>
        /// Get already saved entity detail from storage table
        /// </summary>
        /// <param name="rowKey">rowKey received from bot based on which appropriate row data will be fetched</param>
        /// <returns><see cref="Task"/> Already saved entity detail</returns>
        Task<TicketEntity> GetSavedTicketEntityDetailAsync(string rowKey);
    }
}
