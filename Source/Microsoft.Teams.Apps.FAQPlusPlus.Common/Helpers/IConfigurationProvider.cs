// <copyright file="IConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Configuration App
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Save or update entity based on entity type.
        /// </summary>
        /// <param name="updatedData">updatedData received from view page</param>
        /// <param name="entityType">entityType received from view based on which appropriate row will replaced or inserted in table storage</param>
        /// <returns><see cref="Task"/> boolean value that represents if updated data is saved or updated successfully or not.</returns>
        Task<bool> SaveOrUpdateEntityAsync(string updatedData, string entityType);

        /// <summary>
        /// Get already saved entity detail from storage table
        /// </summary>
        /// <param name="entityType">entityType received from view based on which appropriate row data will be fetched</param>
        /// <returns><see cref="Task"/> Already saved entity detail</returns>
        Task<string> GetSavedEntityDetailAsync(string entityType);

        /// <summary>
        /// Gets Knowledge base details.
        /// </summary>
        /// <param name="kbId">knowledge base id</param>
        /// <returns>Task that resolves to <see cref="GetKnowledgeBaseDetailsResponse"/>.</returns>
        Task<GetKnowledgeBaseDetailsResponse> GetKnowledgeBaseDetailsAsync(string kbId);

        /// <summary>
        /// Check if provided knowledgebase Id is valid or not.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledge base id</param>
        /// <returns><see cref="Task"/> boolean value indicating provided knowledgebase Id is valid or not</returns>
        Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId);
    }
}
