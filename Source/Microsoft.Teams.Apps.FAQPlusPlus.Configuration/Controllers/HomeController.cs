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
        private ConfigurationProvider configurationPovider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configurationPovider">configurationPovider DI.</param>
        public HomeController(ConfigurationProvider configurationPovider)
        {
            this.configurationPovider = configurationPovider;
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
            bool saved = await this.configurationPovider.SaveOrUpdateTeamIdAsync(teamId);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save team Id due to internal server error. Try again");
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
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string knowledge Id</param>
        /// <returns>View</returns>
        public async Task<ActionResult> SaveOrUpdateKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            try
            {
                bool saved = await this.knowledgeBaseHelper.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseId);
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
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string knowledge Id</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> ValidateAndSaveKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            try
            {
                bool isValidKnowledgeBaseId = await this.knowledgeBaseHelper.IsKnowledgeBaseIdValid(knowledgeBaseId);
                if (isValidKnowledgeBaseId)
                {
                    return await this.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseId);
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