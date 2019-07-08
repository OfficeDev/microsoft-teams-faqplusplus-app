// <copyright file="IConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface of Configuration App
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Save or update team Id.
        /// </summary>
        /// <param name="teamId">Team Id received from view page</param>
        /// <returns><see cref="Task"/> boolean value that represents if team Id is saved or updated successfully or not.</returns>
        Task<bool> SaveOrUpdateTeamIdAsync(string teamId);

        /// <summary>
        /// Get already saved team Id from storage table
        /// </summary>
        /// <returns><see cref="Task"/> Already saved team Id.</returns>
        Task<string> GetSavedTeamIdAsync();
    }
}
