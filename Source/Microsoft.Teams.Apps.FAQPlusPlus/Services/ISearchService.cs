// <copyright file="ISearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Search Service provider
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Provide search result for table to be used by SME based on Azure search service.
        /// </summary>
        /// <param name="searchQuery">searchQuery to be provided by message extension</param>
        /// <param name="tabType">tabType to be provided by message extension</param>
        /// <returns><see cref="Task"/> list indicating azure search service results based on searchQuery and tabType</returns>
        Task<IList<TicketEntity>> SMESearchServiceForMessageExtension(string searchQuery, string tabType);
    }
}