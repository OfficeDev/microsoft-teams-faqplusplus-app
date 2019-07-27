// <copyright file="StaticTabController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;

    [ApiController]
    public class StaticTabController : ControllerBase
    {
        private readonly IConfigurationProvider configurationProvider;

        public StaticTabController(IConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Get already saved static tab message from table storage
        /// </summary>
        /// <returns>Static tab text</returns>
        [Route("api/statictab")]
        public async Task<string> GetSavedStaticTabTextAsync()
        {
            return await this.configurationProvider.GetSavedEntityDetailAsync(Constants.StaticTabEntityType);
        }
    }
}