// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System.Net;
    using System.Text.RegularExpressions;
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided team ID is not valid.");
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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the team ID due to an internal error. Try again.");
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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the knowledge base ID due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Validate knowledge base Id from QnA Maker service first and then proceed to save it on success.
        /// The QnA Maker endpoint key is also refreshed as part of this process.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string knowledge Id</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> ValidateAndSaveKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool isValidKnowledgeBaseId = await this.IsKnowledgeBaseIdValid(knowledgeBaseId);
            if (isValidKnowledgeBaseId)
            {
                var endpointRefreshStatus = await this.RefreshQnAMakerEndpointKeyAsync();
                if (!endpointRefreshStatus)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the QnAMaker endpoint key due to an internal error. Try again.");
                }

                return await this.SaveOrUpdateKnowledgeBaseIdAsync(knowledgeBaseId);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided knowledge base ID is not valid.");
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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the welcome message due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved Welcome message from table storage
        /// </summary>
        /// <returns>Welcome message</returns>
        public async Task<string> GetSavedWelcomeMessageAsync()
        {
            var welcomeText = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
            if (welcomeText.Equals(string.Empty))
            {
                await this.SaveWelcomeMessageAsync(Strings.DefaultWelcomeMessage);
            }

            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
        }

        /// <summary>
        /// Save or update help tab text to be used by bot in table storage which is received from View
        /// </summary>
        /// <param name="helpTabText">help tab text</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveHelpTabTextAsync(string helpTabText)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(helpTabText, ConfigurationEntityTypes.HelpTabText);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the help tab text due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved help tab message from table storage
        /// </summary>
        /// <returns>Help tab text</returns>
        public async Task<string> GetSavedHelpTabTextAsync()
        {
            var helpText = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.HelpTabText);
            if (helpText.Equals(string.Empty))
            {
                await this.SaveHelpTabTextAsync(Strings.DefaultHelpTabText);
            }

            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.HelpTabText);
        }

        /// <summary>
        /// Based on deep link URL received find team id and return it to that it can be saved
        /// </summary>
        /// <param name="teamIdDeepLink">team Id deep link</param>
        /// <returns>team Id as string</returns>
        private string ParseTeamIdFromDeepLink(string teamIdDeepLink)
        {
            // team id regex match
            // for a pattern like https://teams.microsoft.com/l/team/19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2/conversations?groupId=53b4782c-7c98-4449-993a-441870d10af9&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47
            // regex checks for 19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2
            var match = Regex.Match(teamIdDeepLink, @"teams.microsoft.com/l/team/(\S+)/");

            if (!match.Success)
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(match.Groups[1].Value);
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
        /// Update the saved endpoint key
        /// </summary>
        /// <returns>Tracking task</returns>
        private async Task<bool> RefreshQnAMakerEndpointKeyAsync()
        {
            try
            {
                var endpointKeys = await this.qnaMakerClient.EndpointKeys.GetKeysAsync();
                await this.configurationPovider.SaveOrUpdateEntityAsync(endpointKeys.PrimaryEndpointKey, ConfigurationEntityTypes.QnAMakerEndpointKey);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}