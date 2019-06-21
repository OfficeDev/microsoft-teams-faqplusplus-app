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
        private KnowledgeBaseHelper knowledgeBaseHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="teamHelper">Team Helper.</param>
        /// <param name="knowledgeBaseHelper">knowledge Base Helper.</param>
        public HomeController(TeamHelper teamHelper, KnowledgeBaseHelper knowledgeBaseHelper)
        {
            this.teamHelper = teamHelper;
            this.knowledgeBaseHelper = knowledgeBaseHelper;
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
        /// <param name="knowledgeBaseIdTextBox">knowledgeBaseIdTextBox is the unique string knowledge Id</param>
        /// <returns>View</returns>
        public async Task<ActionResult> SaveOrUpdateKnowledgeBaseIdAsync(string knowledgeBaseIdTextBox)
        {
            try
            {
                bool saved = await this.knowledgeBaseHelper.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseIdTextBox);
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
        /// Validate knowledge base Id from QnA Maker service first and then proceed to save it on success
        /// </summary>
        /// <param name="knowledgeBaseIdTextBox">knowledgeBaseIdTextBox is the unique string knowledge Id</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> ValidateAndSaveKnowledgeBaseIdAsync(string knowledgeBaseIdTextBox)
        {
            try
            {
                bool isValidKnowledgeBaseId = await this.knowledgeBaseHelper.IsKnowledgeBaseIdValid(knowledgeBaseIdTextBox);
                if (isValidKnowledgeBaseId)
                {
                    return await this.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseIdTextBox);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sorry, provided knowledge base Id is not valid");
                }
            }
            catch (Exception error)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to validate knowledge base Id due to: " + error.Message);
            }
        }

        /// <summary>
        /// Get already saved knowledge base Id from table storage
        /// </summary>
        /// <returns>knowledge base Id</returns>
        [HttpGet]
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            return await this.knowledgeBaseHelper.GetSavedKnowledgeBaseIdAsync();
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