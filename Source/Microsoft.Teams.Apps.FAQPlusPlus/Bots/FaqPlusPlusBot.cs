// <copyright file="FaqPlusPlusBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.Validations;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json.Linq;
    using IConfigurationProvider = Common.Providers.IConfigurationProvider;

    /// <summary>
    ///  This Class Invokes all Bot Conversation functionalities.
    /// </summary>
    public class FaqPlusPlusBot : ActivityHandler
    {
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
        private readonly MessagingExtension messageExtension;
        private readonly IQnAMakerFactory qnaMakerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="telemetryClient"> Telemetry Client.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="qnaMakerFactory">QnAMaker factory instance</param>
        /// <param name="messageExtension">Messaging extension instance</param>
        public FaqPlusPlusBot(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            IConfiguration configuration,
            IQnAMakerFactory qnaMakerFactory,
            MessagingExtension messageExtension)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.configuration = configuration;
            this.qnaMakerFactory = qnaMakerFactory;
            this.messageExtension = messageExtension;
        }

        /// <inheritdoc/>
        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    return this.OnMessageActivityAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), cancellationToken);

                case ActivityTypes.Invoke:
                    return this.OnInvokeActivityAsync(new DelegatingTurnContext<IInvokeActivity>(turnContext), cancellationToken);

                default:
                    return base.OnTurnAsync(turnContext, cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                this.telemetryClient.TrackTrace("Starting Message Activity");

                await this.DisplayTypingIndicator(turnContext);

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
                        var unrecognizedTeamInputCard = UnrecognizedTeamInput.GetCard();
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(unrecognizedTeamInputCard));
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

        /// <inheritdoc/>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var activity = turnContext.Activity;

                this.telemetryClient.TrackTrace($"Received conversationUpdateActivity");
                this.telemetryClient.TrackTrace($"conversationType: {activity.Conversation?.ConversationType}, membersAdded: {activity.MembersAdded?.Count()}, membersRemoved: {activity.MembersRemoved?.Count()}");

                if (activity.MembersAdded?.Count() > 0)
                {
                    switch (activity.Conversation.ConversationType)
                    {
                        case "personal":
                            await this.OnMembersAddedToPersonalChatAsync(activity.MembersAdded, turnContext, cancellationToken);
                            break;

                        case "channel":
                            await this.OnMembersAddedToTeamAsync(activity.MembersAdded, turnContext, cancellationToken);
                            break;

                        default:
                            this.telemetryClient.TrackTrace($"Ignoring event from conversation type {activity.Conversation.ConversationType}");
                            break;
                    }
                }
                else
                {
                    this.telemetryClient.TrackTrace($"Ignoring conversationUpdate that was not a membersAdded event");
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"Error processing conversationUpdate: {ex.Message}", SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
            }
        }

        // Handles members added conversationUpdate event in 1:1 chat
        private async Task OnMembersAddedToPersonalChatAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                // User started chat with the bot in personal scope, for the first time
                this.telemetryClient.TrackTrace($"Bot added to 1:1 chat {activity.Conversation.Id}");

                var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
                var userWelcomeCardAttachment = await WelcomeCard.GetCard(welcomeText);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));
            }
        }

        // Handles members added conversationUpdate event in team
        private async Task OnMembersAddedToTeamAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                // Bot was added to a team
                this.telemetryClient.TrackTrace($"Bot added to team {activity.Conversation.Id}");

                var teamDetails = ((JObject)turnContext.Activity.ChannelData).ToObject<TeamsChannelData>();
                var botDisplayName = turnContext.Activity.Recipient.Name;
                var teamWelcomeCardAttachment = WelcomeTeamCard.GetCard(botDisplayName, teamDetails.Team.Name);
                await this.NotifyTeam(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken);
            }
        }

        /// <summary>
        /// The method that gets invoked when activity is of type Invoke is received from bot.
        /// </summary>
        /// <param name="turnContext">The current turn of invoke activity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private async Task OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var responseActivity = new Activity(ActivityTypesEx.InvokeResponse);

            switch (turnContext.Activity.Name)
            {
                case "composeExtension/query":
                    var invokeResponse = await this.messageExtension.HandleMessagingExtensionQueryAsync(turnContext).ConfigureAwait(false);
                    responseActivity.Value = invokeResponse;
                    break;

                default:
                    this.telemetryClient.TrackTrace($"Received invoke activity with unknown name {turnContext.Activity.Name}");
                    responseActivity.Value = new InvokeResponse { Status = 200 };
                    break;
            }

            await turnContext.SendActivityAsync(responseActivity).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends update to the user in adaptive cards, after bot posting user query to SME channel.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="updateActivityAttachment">Activity update adaptive card attachment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Thank you Card.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateFeedbackActivity(ITurnContext turnContext, Attachment updateActivityAttachment, CancellationToken cancellationToken)
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
        private List<Attachment> CreateTeamTourCardCarouselAttachment()
        {
            return new List<Attachment>()
            {
                TourCarousel.GetCard(Resource.TeamFunctionCardHeaderText, Resource.TeamFunctionCardContent, this.configuration["AppBaseUri"] + "/content/Alert.png"),
                TourCarousel.GetCard(Resource.TeamChatHeaderText, Resource.TeamChatCardContent, this.configuration["AppBaseUri"] + "/content/Userchat.png"),
                TourCarousel.GetCard(Resource.TeamQueryHeaderText, Resource.TeamQueryCardContent, this.configuration["AppBaseUri"] + "/content/Ticket.png"),
            };
        }

        /// <summary>
        /// Displays Carousel of Tour Cards- for personal scope.
        /// </summary>
        /// <returns>The Tour Adaptive card.</returns>
        private List<Attachment> CreateUserTourCardCarouselAttachment()
        {
            return new List<Attachment>()
            {
                TourCarousel.GetCard(Resource.FunctionCardText1, Resource.FunctionCardText2, this.configuration["AppBaseUri"] + "/content/Qnamaker.png"),
                TourCarousel.GetCard(Resource.AskAnExpertText1, Resource.AskAnExpertText2, this.configuration["AppBaseUri"] + "/content/Askanexpert.png"),
                TourCarousel.GetCard(Resource.ShareFeedbackTitleText, Resource.FeedbackText1, this.configuration["AppBaseUri"] + "/content/Shareappfeedback.png"),
            };
        }

        /// <summary>
        /// Method that gets an answer from the QnAMaker resource.
        /// </summary>
        /// <param name="kbId">Knowledgebase Id.</param>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>A unit of execution.</returns>
        private async Task GetAnswersAsync(string kbId, ITurnContext<IMessageActivity> turnContext)
        {
            var qnaMaker = this.qnaMakerFactory.GetQnAMaker(kbId);
            var options = new QnAMakerOptions { Top = Top, ScoreThreshold = float.Parse(this.configuration["ScoreThreshold"]) };
            var response = await qnaMaker.GetAnswersAsync(turnContext, options);
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseAdaptiveCard.GetCard(response[0].Questions[0], response[0].Answer, turnContext.Activity.Text)));
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Notification to SME team channel.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task BroadcastTeamMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var payload = ((JObject)turnContext.Activity.Value).ToObject<UserActivity>();
            var channelAccountDetails = await this.GetPersonalChatUserAccountDetailsAsync(turnContext, cancellationToken);
            var fullName = turnContext.Activity.From.Name;
            Attachment teamCardAttachment = null;
            string activityType = string.IsNullOrEmpty(turnContext.Activity.Text) ? string.Empty : turnContext.Activity.Text.Trim().ToLower();
            switch (activityType)
            {
                case AppFeedback:
                    teamCardAttachment = this.GetAppFeedbackAttachment(channelAccountDetails, payload, fullName);
                    break;

                case QuestionForExpert:
                    teamCardAttachment = this.GetQuestionForExpertAttachment(channelAccountDetails, payload, fullName);
                    break;

                case ResultsFeedback:
                    teamCardAttachment = this.GetResultsFeedbackAttachment(channelAccountDetails, payload, fullName);
                    break;

                default:
                    break;
            }

            var channelId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
            await this.NotifyTeam(turnContext, teamCardAttachment, channelId, cancellationToken);
            if (!string.IsNullOrEmpty(payload.QuestionUserTitleText))
            {
                await this.UpdateFeedbackActivity(turnContext, NotificationCard.GetCard(payload.QuestionForExpert, payload.QuestionUserTitleText), cancellationToken);
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
        private async Task<TeamsChannelAccount> GetPersonalChatUserAccountDetailsAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken);
            return members[0].Properties.ToObject<TeamsChannelAccount>();
        }

        /// <summary>
        /// This method displays typing indicator for user when bot is interacting with SME team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <returns>Displays Typing Indicator to the user while the message is sent to the SME channel.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DisplayTypingIndicator(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            await turnContext.SendActivityAsync(typingActivity);
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
                await this.BroadcastTeamMessage(turnContext, cancellationToken);
            }
        }

        private Attachment GetAppFeedbackAttachment(TeamsChannelAccount channelAccountDetails, UserActivity userActivityPayload, string fullName)
        {
            var incomingSubtitleText = string.Format(Resource.IncomingFeedbackSubHeaderText, fullName, Resource.AppFeedbackText);
            return IncomingSMEEnquiryCard.GetCard(Resource.AppFeedbackText, userActivityPayload.FeedbackUserTitleText, incomingSubtitleText, channelAccountDetails, userActivityPayload);
        }

        private Attachment GetQuestionForExpertAttachment(TeamsChannelAccount channelAccountDetails, UserActivity userActivityPayload, string fullName)
        {
            var incomingSubtitleText = string.Format(Resource.QuestionForExpertSubHeaderText, fullName, Resource.QuestionForExpertText);
            return IncomingSMEEnquiryCard.GetCard(Resource.QuestionForExpertText, userActivityPayload.QuestionUserTitleText, incomingSubtitleText, channelAccountDetails, userActivityPayload, true);
        }

        private Attachment GetResultsFeedbackAttachment(TeamsChannelAccount channelAccountDetails, UserActivity userActivityPayload, string fullName)
        {
            var incomingSubtitleText = string.Format(Resource.IncomingFeedbackSubHeaderText, fullName, Resource.ResultsFeedbackText);
            return IncomingSMEEnquiryCard.GetCard(Resource.ResultsFeedbackText, userActivityPayload.FeedbackUserTitleText, incomingSubtitleText, channelAccountDetails, userActivityPayload);
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
                    var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
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
                            var kbID = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId);

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