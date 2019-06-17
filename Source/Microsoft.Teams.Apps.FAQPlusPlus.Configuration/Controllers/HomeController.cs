// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private TeamHelper teamHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="teamHelper">Team Helper.</param>
        public HomeController(TeamHelper teamHelper)
        {
            this.teamHelper = teamHelper;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns>View</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// SaveTeamIdAsync
        /// </summary>
        /// <param name="teamId">team Id is the unique string associated with each team</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveTeamIdAsync(string teamId)
        {
            try
            {
                bool saved = await this.teamHelper.SaveTeamIdDetailAsync(teamId);
                if (saved)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sorry, unable to save data since Team Id already exists or server returned HTTP status code 204");
                }
            }
            catch (Exception error)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save data due to: " + error.Message);
            }
        }

        /// <summary>
        /// GetSavedTeamIdAsync
        /// </summary>
        /// <returns>Team Id</returns>
        [HttpGet]
        public async Task<string> GetSavedTeamIdAsync()
        {
            return await this.teamHelper.GetSavedTeamIdAsync();
        }

        /// <summary>
        /// SaveKnowledgeBaseUrl
        /// </summary>
        /// <param name="knowledgeBaseUrl">KnowledgeBaseUrl</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult SaveKnowledgeBaseUrl(string knowledgeBaseUrl)
        {
            // Default placeholder for implementation. Will be changed once its related changes implemented
            // To be changed to Async method
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// SaveUpnEmailAddress
        /// </summary>
        /// <param name="upnEmailAddress">upnEmailAddress</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult SaveUpnEmailAddress(string upnEmailAddress)
        {
            // Default placeholder for implementation. Will be changed once its related changes implemented
            // To be changed to Async method
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}