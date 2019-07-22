// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private const string TeamIdStartString = "19%3a";
        private const string TeamIdEndString = "%40thread.skype";

        private readonly ConfigurationProvider configurationPovider;
        private readonly QnAMakerService qnaMakerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configurationPovider">configurationPovider DI.</param>
        /// <param name="qnaMakerService">qnaMakerService DI.</param>
        public HomeController(ConfigurationProvider configurationPovider, QnAMakerService qnaMakerService)
        {
            this.configurationPovider = configurationPovider;
            this.qnaMakerService = qnaMakerService;
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
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(teamId, Constants.TeamEntityType);
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
            return await this.configurationPovider.GetSavedEntityDetailAsync(Constants.TeamEntityType);
        }

        /// <summary>
        /// Save or update knowledgeBaseId in table storage which is received from View
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string knowledge Id</param>
        /// <returns>View</returns>
        public async Task<ActionResult> SaveOrUpdateKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(knowledgeBaseId, Constants.KnowledgeBaseEntityType);
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
        /// Get already saved knowledge base Id from table storage
        /// </summary>
        /// <returns>knowledge base Id</returns>
        [HttpGet]
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityType);
        }

        /// <summary>
        /// Save or update welcome message to be used by bot in table storage which is received from View
        /// </summary>
        /// <param name="welcomeMessage">welcomeMessage</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveWelcomeMessageAsync(string welcomeMessage)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(welcomeMessage, Constants.WelcomeMessageEntityType);
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
            return await this.configurationPovider.GetSavedEntityDetailAsync(Constants.WelcomeMessageEntityType);
        }

        /// <summary>
        /// Save or update static tab text to be used by bot in table storage which is received from View
        /// </summary>
        /// <param name="staticTabText">staticTabText</param>
        /// <returns>View</returns>
        [HttpPost]
        public async Task<ActionResult> SaveStaticTabTextAsync(string staticTabText)
        {
            bool saved = await this.configurationPovider.SaveOrUpdateEntityAsync(staticTabText, Constants.StaticTabEntityType);
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
        [AllowAnonymous]
        public async Task<string> GetSavedStaticTabTextAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(Constants.StaticTabEntityType);
        }
    }
}