// <copyright file="IQnAMakerService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of QnA Maker Service
    /// </summary>
    public interface IQnAMakerService
    {
        /// <summary>
        /// Gets Knowledge base details.
        /// </summary>
        /// <param name="kbId">knowledge base id</param>
        /// <returns>Task that resolves to <see cref="GetKnowledgeBaseDetailsResponse"/>.</returns>
        Task<GetKnowledgeBaseDetailsResponse> GetKnowledgeBaseDetails(string kbId);
    }
}
