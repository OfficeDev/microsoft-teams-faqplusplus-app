// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private const string TeamIdEscapedStartString = "19%3a";
        private const string TeamIdEscapedEndString = "%40thread.skype";
        private const string TeamIdUnescapedStartString = "19:";
        private const string TeamIdUnescapedEndString = "@thread.skype";

        private readonly ConfigurationProvider configurationPovider;
        private readonly IQnAMakerClient qnaMakerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configurationPovider">configurationPovider DI.</param>
        /// <param name="qnaMakerClient">qnaMakerClient DI.</param>
        public HomeController(ConfigurationProvider configurationPovider, IQnAMakerClient qnaMakerClient)
        {
            this.configurationPovider = configurationPovider;
            this.qnaMakerClient = qnaMakerClient;
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
        /// Parse team Id from first and then proceed to save it on success
        /// </summary>
        /// <param name="teamId">teamId is the unique string</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> ParseAndSaveTeamIdAsync(string teamId)
        {
            string teamIdAfterParse = this.ParseTeamIdFromDeepLink(teamId);
            if (!string.IsNullOrEmpty(teamIdAfterParse))
            {
                return await this.SaveOrUpdateTeamIdAsync(teamIdAfterParse);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, provided team ID is not valid.");
            }
        }

        /// <summary>
        /// Save or update teamId in table storage which is received from View
        /// </summary>
        /// <param name="teamId">teamId is the unique deep link URL string associated with each team</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveOrUpdateTeamIdAsync(string teamId)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(teamId, ConfigurationEntityTypes.TeamId);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save team ID due to internal server error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved team Id from table storage
        /// </summary>
        /// <returns>Team Id</returns>
        [HttpGet]
        public async Task<string> GetSavedTeamIdAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
        }

        /// <summary>
        /// Save or update knowledgeBaseId in table storage which is received from View
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string knowledge Id</param>
        /// <returns>View</returns>
        public async Task<ActionResult> SaveOrUpdateKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(knowledgeBaseId, ConfigurationEntityTypes.KnowledgeBaseId);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save knowledge base ID due to internal server error. Try again.");
            }
        }

        /// <summary>
        /// Save or update Endpoint key in table storage which is received from View
        /// </summary>
        /// <param name="endpointKey">Endpoint key </param>
        /// <returns>View</returns>
        public async Task<ActionResult> SaveOrUpdateEndpointKeyAsync(string endpointKey)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(endpointKey, ConfigurationEntityTypes.EndpointKey);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save endpoint key due to internal server error. Try again.");
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
            bool isValidKnowledgeBaseId = await this.IsKnowledgeBaseIdValid(knowledgeBaseId);
            if (isValidKnowledgeBaseId)
            {
                return await this.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseId);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, provided knowledge base ID is not valid.");
            }
        }

        /// <summary>
        /// Validate Endpoint key from QnA Maker service first and then proceed to save it on success
        /// </summary>
        /// <param name="endpointKey">Endpoint key </param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> ValidateAndSaveEndpointKeyAsync(string endpointKey)
        {
            bool isValidKnowledgeBaseId = await this.IsEndpointKeyValid(endpointKey);
            if (isValidKnowledgeBaseId)
            {
                return await this.SaveOrUpdateEndpointKeyAsync(endpointKey);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, provided endpoint key is not valid.");
            }
        }

        /// <summary>
        /// Get already saved knowledge base Id from table storage
        /// </summary>
        /// <returns>knowledge base Id</returns>
        [HttpGet]
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId);
        }

        /// <summary>
        /// Get already saved endpoint key from table storage
        /// </summary>
        /// <returns>endpoint key</returns>
        [HttpGet]
        public async Task<string> GetSavedEndpointKeyAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.EndpointKey);
        }

        /// <summary>
        /// Save or update welcome message to be used by bot in table storage which is received from View
        /// </summary>
        /// <param name="welcomeMessage">welcomeMessage</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveWelcomeMessageAsync(string welcomeMessage)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(welcomeMessage, ConfigurationEntityTypes.WelcomeMessageText);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save welcome message due to internal server error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved Welcome message from table storage
        /// </summary>
        /// <returns>Welcome message</returns>
        public async Task<string> GetSavedWelcomeMessageAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
        }

        /// <summary>
        /// Save or update static tab text to be used by bot in table storage which is received from View
        /// </summary>
        /// <param name="staticTabText">staticTabText</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveStaticTabTextAsync(string staticTabText)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(staticTabText, ConfigurationEntityTypes.StaticTabText);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save static tab text due to internal server error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved static tab message from table storage
        /// </summary>
        /// <returns>Static tab text</returns>
        public async Task<string> GetSavedStaticTabTextAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.StaticTabText);
        }

        /// <summary>
        /// Based on deep link URL received find team id and return it to that it can be saved
        /// </summary>
        /// <param name="teamIdDeepLink">team Id deep link</param>
        /// <returns>team Id as string</returns>
        private string ParseTeamIdFromDeepLink(string teamIdDeepLink)
        {
            int startEscapedIndex = teamIdDeepLink.IndexOf(TeamIdEscapedStartString, StringComparison.OrdinalIgnoreCase);
            int endEscapedIndex = teamIdDeepLink.IndexOf(TeamIdEscapedEndString, StringComparison.OrdinalIgnoreCase);

            int startUnescapedIndex = teamIdDeepLink.IndexOf(TeamIdUnescapedStartString, StringComparison.OrdinalIgnoreCase);
            int endUnescapedIndex = teamIdDeepLink.IndexOf(TeamIdUnescapedEndString, StringComparison.OrdinalIgnoreCase);

            string teamID = string.Empty;

            if (startEscapedIndex > -1 && endEscapedIndex > -1)
            {
                teamID = HttpUtility.UrlDecode(teamIdDeepLink.Substring(startEscapedIndex, endEscapedIndex - startEscapedIndex + TeamIdEscapedEndString.Length));
            }
            else if (startUnescapedIndex > -1 && endUnescapedIndex > -1)
            {
                teamID = teamIdDeepLink.Substring(startUnescapedIndex, endUnescapedIndex - startUnescapedIndex + TeamIdUnescapedEndString.Length);
            }

            return teamID;
        }

        /// <summary>
        /// Check if provided knowledgebase Id is valid or not.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledge base id</param>
        /// <returns><see cref="Task"/> boolean value indicating provided knowledgebase Id is valid or not</returns>
        private async Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId)
        {
            try
            {
                var kbIdDetail = await this.qnaMakerClient.Knowledgebase.GetDetailsAsync(knowledgeBaseId);

                return kbIdDetail.Id == knowledgeBaseId;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if provided Endpoint key is valid or not.
        /// </summary>
        /// <param name="endpointKey">Endpoint key</param>
        /// <returns><see cref="Task"/> boolean value indicating provided endpoint key is valid or not</returns>
        private async Task<bool> IsEndpointKeyValid(string endpointKey)
        {
            try
            {
                var endpointKeys = await this.qnaMakerClient.EndpointKeys.GetKeysWithHttpMessagesAsync();

                return endpointKeys.Body.PrimaryEndpointKey == endpointKey;
            }
            catch
            {
                return false;
            }
        }
    }
}