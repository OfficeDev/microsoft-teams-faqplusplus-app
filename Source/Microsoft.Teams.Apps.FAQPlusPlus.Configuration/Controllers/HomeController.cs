﻿// <copyright file="HomeController.cs" company="Microsoft">
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
        /// The landing page
        /// </summary>
        /// <returns>Default landing page view</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Save or update teamId in table storage which is received from View
        /// </summary>
        /// <param name="teamId">teamId is the unique string associated with each team</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveOrUpdateTeamIdAsync(string teamId)
        {
            try
            {
                bool saved = await this.teamHelper.SaveOrUpdateTeamIdAsync(teamId);
                if (saved)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sorry, unable to save data due to HTTP status code 204");
                }
            }
            catch (Exception error)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save data due to: " + error.Message);
            }
        }

        /// <summary>
        /// Get already saved team Id from table storage
        /// </summary>
        /// <returns>Team Id</returns>
        [HttpGet]
        public async Task<string> GetSavedTeamIdAsync()
        {
            return await this.teamHelper.GetSavedTeamIdAsync();
        }

        /// <summary>
        /// Save or update knowledgeBaseId in table storage which is received from View
        /// </summary>
        /// <param name="knowledgeBaseId">KnowledgeBaseId</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult SaveOrUpdateKnowledgeBaseId(string knowledgeBaseId)
        {
            // Default placeholder for implementation. Will be changed once its related changes implemented            
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Save or update upnEmailAddress in table storage which is received from View
        /// </summary>
        /// <param name="upnEmailAddress">upnEmailAddress</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult SaveOrUpdateUpnEmailAddress(string upnEmailAddress)
        {
            // Default placeholder for implementation. Will be changed once its related changes implemented            
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}