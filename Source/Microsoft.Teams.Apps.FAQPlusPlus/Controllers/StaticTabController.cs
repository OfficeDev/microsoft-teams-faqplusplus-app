// <copyright file="StaticTabController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This is a Static tab controller class which will be used to display Help
    /// details in the bot tab.
    /// </summary>
    public class StaticTabController : Controller
    {
        private readonly IConfigurationProvider configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticTabController"/> class.
        /// </summary>
        /// <param name="configurationProvider">configurationProvider DI</param>
        public StaticTabController(IConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Get already saved static tab message from table storage
        /// </summary>
        /// <returns>Static tab text</returns>
        [Route("/Help")]
        public async Task<ActionResult> GetSavedStaticTabTextAsync()
        {
            string helpTabText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.StaticTabText);

            var marked = new MarkedNet.Marked();
            var htmlTabHtml = marked.Parse(helpTabText);

            return this.View("~/Views/StaticTab/StaticTabContent.cshtml", htmlTabHtml);
        }
    }
}