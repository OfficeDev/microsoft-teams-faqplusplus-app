// <copyright file="ITicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Interface of Tickets provider
    /// </summary>
    public interface ITicketsProvider
    {
        /// <summary>
        /// Save or update ticket entity.
        /// </summary>
        /// <param name="ticketEntity">ticketEntity received from bot based on which appropriate row will replaced or inserted in table storage</param>
        /// <returns><see cref="Task"/> boolean value that represents if updated data is saved or updated successfully or not.</returns>
        Task<bool> SaveOrUpdateTicketEntityAsync(TicketEntity ticketEntity);

        /// <summary>
        /// Get already saved entity detail from storage table
        /// </summary>
        /// <param name="rowKey">rowKey received from bot based on which appropriate row data will be fetched</param>
        /// <returns><see cref="Task"/> Already saved entity detail</returns>
        Task<TableResult> GetSavedTicketEntityDetailAsync(string rowKey);
    }
}
