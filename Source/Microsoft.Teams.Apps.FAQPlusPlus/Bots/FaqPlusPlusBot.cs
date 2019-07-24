// <copyright file="FaqPlusPlusBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.Validations;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json.Linq;
    using IConfigurationProvider = Common.Helpers.IConfigurationProvider;

    /// <summary>
    ///  This Class Invokes all Bot Conversation functionalities.
    /// </summary>
    public class FaqPlusPlusBot : ActivityHandler
    {
        public const string KnowledgeBase = "KnowledgeBase";
        public const string WelcomeMessage = "WelcomeMessage";
        private const string TakeATour = "take a tour";
        private const string AskAnExpert = "ask an expert";
        private const string Feedback = "share feedback";
        private const string TeamTour = "team tour";
        private const string AppFeedback = "appfeedback";
        private const string ResultsFeedback = "resultsfeedback";
        private const string QuestionForExpert = "questionforexpert";
        private static readonly int Top = 1;
        private readonly IConfiguration configuration;
        private readonly TelemetryClient telemetryClient;
        private readonly IConfigurationProvider configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="telemetryClient"> Telemetry Client.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="client">Http Client.</param>
        public FaqPlusPlusBot(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            IConfiguration configuration)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.configuration = configuration;
        }

        /// <summary>
        /// Sends update to the user in adaptive cards, after bot posting user query to SME channel.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="updateActivityAttachment">Activity update adaptive card attachment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Thank you Card.<see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateFeedbackActivity(ITurnContext turnContext, Attachment updateActivityAttachment, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply();
            reply.Attachments = new List<Attachment>()
            {
                updateActivityAttachment,
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Displays Carousel of Tour Cards when bot is added to a team scope.
        /// </summary>
        /// <returns>The Tour Adaptive card.</returns>
        public List<Attachment> CreateTeamTourCardCarouselAttachment()
        {
            return new List<Attachment>()
            {
                TourCarousel.GetCard(Resource.TeamFunctionCardHeaderText, Resource.TeamFunctionCardContent, this.configuration["AppBaseUri"] + "/content/Alert.png"),
                TourCarousel.GetCard(Resource.TeamChatHeaderText, Resource.TeamChatCardContent, this.configuration["AppBaseUri"] + "/content/UserChat.png"),
                TourCarousel.GetCard(Resource.TeamQueryHeaderText, Resource.TeamQueryCardContent, this.configuration["AppBaseUri"] + "/content/Ticket.png"),
            };
        }

        /// <summary>
        /// Displays Carousel of Tour Cards- for personal scope.
        /// </summary>
        /// <returns>The Tour Adaptive card.</returns>
        public List<Attachment> CreateUserTourCardCarouselAttachment()
        {
            return new List<Attachment>()
            {
                TourCarousel.GetCard(Resource.FunctionCardText1, Resource.FunctionCardText2, this.configuration["AppBaseUri"] + "/content/QnaMaker.png"),
                TourCarousel.GetCard(Resource.AskAnExpertText1, Resource.AskAnExpertText2, this.configuration["AppBaseUri"] + "/content/AskAnExpert.png"),
                TourCarousel.GetCard(Resource.ShareFeedbackTitleText, Resource.FeedbackText1, this.configuration["AppBaseUri"] + "/content/ShareFeedback.png"),
            };
        }

        /// <summary>
        /// Method that gets an answer from the QnAMaker resource.
        /// </summary>
        /// <param name="kbId">Knowledgebase Id.</param>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>A unit of execution.</returns>
        public async Task GetAnswersAsync(string kbId, ITurnContext<IMessageActivity> turnContext)
        {
            var qnaMaker = new QnAMaker(
                new Bot.Configuration.QnAMakerService()
                {
                    KbId = kbId,
                    EndpointKey = this.configuration["EndpointKey"],
                    Hostname = this.configuration["KbHost"],
                    SubscriptionKey = this.configuration["QnAMakerSubscriptionKey"],
                },
            new QnAMakerOptions { Top = Top, ScoreThreshold = float.Parse(this.configuration["ScoreThreshold"]) });

            // The actual call to the QnA Maker service.
            var response = await qnaMaker.GetAnswersAsync(turnContext);
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseAdaptiveCard.GetCard(response[0].Questions[0], response[0].Answer)));
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInput.GetCard(turnContext.Activity.Text)));
            }
        }

        /// <summary>
        /// Sends the message to SME team upon collecting feedback or question from the user.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Notification to SME team channel.<see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task BroadcastTeamMessage(ITurnContext<IMessageActivity> turnContext, IConfigurationProvider configurationProvider, CancellationToken cancellationToken)
        {
            var payload = ((JObject)turnContext.Activity.Value).ToObject<UserActivity>();
            var channelAccountDetails = this.GetTeamsChannelAccountDetails(turnContext, cancellationToken);
            var fullName = turnContext.Activity.Recipient.Name;
            Attachment teamCardAttachment = null;
            string activityType = string.IsNullOrEmpty(turnContext.Activity.Text) ? string.Empty : turnContext.Activity.Text.Trim().ToLower();
            switch (activityType)
            {
                case AppFeedback:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("App Feedback", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.AppFeedback);
                    break;

                case QuestionForExpert:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("Question For Expert", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.QuestionForExpert);
                    break;

                case ResultsFeedback:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("Results Feedback", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.ResultsFeedback, payload.SMEQuestion, payload.SMEAnswer);
                    break;

                default:
                    break;
            }

            await this.DisplayTypingIndicator(turnContext);
            await this.NotifyTeam(turnContext, teamCardAttachment, this.configuration["ChannelId"], cancellationToken);

            if (payload.QuestionForExpert != null)
            {
                await this.UpdateFeedbackActivity(turnContext, ConfirmationCard.GetCard(), cancellationToken);
            }
            else
            {
                await this.UpdateFeedbackActivity(turnContext, ThankYouAdaptiveCard.GetCard(), cancellationToken);
            }
        }

        /// <summary>
        /// This methods gets teams channel account details.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> Team channel account details.<see cref="Task"/> representing the asynchronous operation.</returns>
        public TeamsChannelAccount GetTeamsChannelAccountDetails(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken).GetAwaiter().GetResult();
            return ((JObject)members[0].Properties).ToObject<TeamsChannelAccount>();
        }

        /// <summary>
        /// The method that gets invoked each time there is a message that is coming in.
        /// </summary>
        /// <param name="turnContext">The current turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                this.telemetryClient.TrackTrace("Starting Message Activity");

                // when conversation is from Teams channel
                if (turnContext.Activity.Conversation.ConversationType == "channel")
                {
                    string activityText = string.IsNullOrEmpty(turnContext.Activity.Text) ? string.Empty : turnContext.Activity.Text.Trim().ToLower();
                    this.telemetryClient.TrackTrace($"User entered text = {activityText}");
                    if (activityText == TeamTour)
                    {
                        this.telemetryClient.TrackTrace("Calling TeamTour Card");
                        var teamtourCardCarouselAttachment = await Task.Run(() => this.CreateTeamTourCardCarouselAttachment());
                        await turnContext.SendActivityAsync(MessageFactory.Carousel(teamtourCardCarouselAttachment));
                    }
                    else if (turnContext.Activity.Value != null && ((JObject)turnContext.Activity.Value).Count != 0)
                    {
                        // To do:
                        // await this.SendCardsUsrAsync(turnContext, cancellationToken);
                    }
                    else
                    {
                        var teamtourCardAttachment = UnrecognizedTeamInput.GetCard();
                        await this.SendTeamMessage(turnContext, teamtourCardAttachment, cancellationToken);
                    }
                }
                else if (turnContext.Activity.Value != null && ((JObject)turnContext.Activity.Value).Count != 0 && !string.IsNullOrEmpty(turnContext.Activity.Text))
                {
                    await this.SendCardsToSMEAsync(turnContext, cancellationToken);
                }
                else if (!string.IsNullOrEmpty(turnContext.Activity.Text))
                {
                    await this.SendCardsAsync(turnContext, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// The method that gets invoked when the bot is added to Team or 1:1 scope.
        /// </summary>
        /// <param name="membersAdded">The account that has been either added or interacting with the bot.</param>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of Execution.</returns>
        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                foreach (var member in membersAdded)
                {
                    // When bot is added to a user in personal scope, for the first time.
                    if (member.Id != turnContext.Activity.Recipient.Id)
                    {
                        var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(WelcomeMessage);
                        var userWelcomecardAttachment = await WelcomeCard.GetCard(welcomeText);
                        this.telemetryClient.TrackTrace($"Member Id of User = {member.Id}");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomecardAttachment));
                    }

                    // When bot is added to a team, for the first time.
                    else if (turnContext.Activity.Conversation.ConversationType.ToLower() != "personal")
                    {
                        var teamDetails = ((JObject)turnContext.Activity.ChannelData).ToObject<TeamsChannelData>();
                        var botDisplayName = turnContext.Activity.Recipient.Name;
                        this.telemetryClient.TrackTrace($"Team members are being added: {teamDetails.Team.Id}");
                        var teamWelcomeCardAttachment = WelcomeTeamCard.GetCard(botDisplayName, teamDetails.Team.Name);
                        await this.NotifyTeam(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// Method that fires first when updating any activity in a team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns appropriate adaptive card.<see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var membersAdded = turnContext.Activity.MembersAdded;
            await this.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);
        }

        /// <summary>
        /// This method displays typing indicator for user when bot is interacting with SME team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <returns>Displays Typing Indicator to the user while the message is sent to the SME channel.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DisplayTypingIndicator(ITurnContext turnContext)
        {
            Activity isTypingActivity = turnContext.Activity.CreateReply();
            isTypingActivity.Type = ActivityTypes.Typing;
            await turnContext.SendActivityAsync((Activity)isTypingActivity);
        }

        /// <summary>
        /// Notification to the SME team when bot post a question or feedback to the SME team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="attachmentToSend">sends Adaptive card.</param>
        /// <param name="teamId">Team Id to which the message is being sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Message to the SME Team.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task NotifyTeam(ITurnContext turnContext, Attachment attachmentToSend, string teamId, CancellationToken cancellationToken)
        {
            var teamMessageActivity = new Activity()
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount()
                {
                    Id = teamId,
                },
                Attachments = new List<Attachment>()
                {
                    attachmentToSend,
                },
            };
            await ((BotFrameworkAdapter)turnContext.Adapter).SendActivitiesAsync(turnContext, new Activity[] { teamMessageActivity }, cancellationToken);
        }

        /// <summary>
        /// Generic method sends messages to the team when team member interacts with the bot.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="attachment">Adaptive card attachment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns appropriate adaptive card.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendTeamMessage(ITurnContext<IMessageActivity> turnContext, Attachment attachment, CancellationToken cancellationToken)
        {
            var teamDetails = ((JObject)turnContext.Activity.ChannelData).ToObject<TeamsChannelData>();
            await this.NotifyTeam(turnContext, attachment, teamDetails.Team.Id, cancellationToken);
        }

        /// <summary>
        /// Method sends the user activity information to SME channel.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns appropriate adaptive card.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendCardsToSMEAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var validation = UserInputValidations.Validate(turnContext, cancellationToken);
            if (validation == true)
            {
                await this.BroadcastTeamMessage(
                       turnContext,
                       this.configurationProvider,
                       cancellationToken);
            }
        }

        /// <summary>
        /// Sends the Appropriate Adaptive Card to the user for the respective command.
        /// Or Hits the QnA maker if user has asked a question.
        /// </summary>
        /// <param name="context">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns appropriate adaptive card.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendCardsAsync(ITurnContext<IMessageActivity> context, CancellationToken cancellationToken)
        {
            try
            {
                string activityText = string.IsNullOrEmpty(context.Activity.Text) ? string.Empty : context.Activity.Text.Trim().ToLower();
                this.telemetryClient.TrackTrace($"User entered text = {activityText}");
                if (string.IsNullOrEmpty(activityText))
                {
                    var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(WelcomeMessage);
                    var userWelcomecardAttachment = await WelcomeCard.GetCard(welcomeText);
                    await context.SendActivityAsync(MessageFactory.Text("Hey, I don't understand what you're saying, would you like to take a tour"), cancellationToken);
                    await context.SendActivityAsync(MessageFactory.Attachment(userWelcomecardAttachment));
                }
                else
                {
                    switch (activityText)
                    {
                        case AskAnExpert:
                            this.telemetryClient.TrackTrace("Calling AskAnExpert Card");
                            await context.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard()));
                            break;

                        case Feedback:
                            this.telemetryClient.TrackTrace("Calling Feedback Card");
                            await context.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard()));
                            break;

                        case TakeATour:
                            this.telemetryClient.TrackTrace("Calling TakeATour Card");
                            var tourCardCarouselAttachment = await Task.Run(() => this.CreateUserTourCardCarouselAttachment());
                            await context.SendActivityAsync(MessageFactory.Carousel(tourCardCarouselAttachment));
                            break;

                        default:
                            this.telemetryClient.TrackTrace("Calling QnA Maker Service");
                            var kbID = await this.configurationProvider.GetSavedEntityDetailAsync(KnowledgeBase);

                            // ToDo: Validate Null condition when KB is not available.
                            if (!string.IsNullOrEmpty(kbID))
                            {
                                await this.GetAnswersAsync(kbID, context);
                            }
                            else
                            {
                                await context.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInput.GetCard(context.Activity.Text)));
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
}