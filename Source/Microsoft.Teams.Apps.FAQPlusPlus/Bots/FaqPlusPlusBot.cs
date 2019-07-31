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
        private const string AppFeedback = "AppFeedback";
        private const string ResultsFeedback = "ResultsFeedback";
        private const string QuestionForExpert = "QuestionForExpert";
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
                var message = turnContext.Activity;

                this.telemetryClient.TrackTrace($"Received message activity");
                this.telemetryClient.TrackTrace($"from: {message.From?.Id}, conversation: {message.Conversation.Id}, replyToId: {message.ReplyToId}");

                await this.DisplayTypingIndicator(turnContext);

                switch (message.Conversation.ConversationType)
                {
                    case "personal":
                        await this.OnMessageActivityInPersonalChatAsync(message, turnContext, cancellationToken);
                        break;

                    case "channel":
                        await this.OnMessageActivityInChannelAsync(message, turnContext, cancellationToken);
                        break;

                    default:
                        this.telemetryClient.TrackTrace($"Received unexpected conversationType {message.Conversation.ConversationType}", SeverityLevel.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                // TODO: Respond to the user with an error message
                this.telemetryClient.TrackTrace($"Error processing message: {ex.Message}", SeverityLevel.Error);
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
                await this.SendMessageToTeamAsync(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken);
            }
        }

        // Handles message activity in 1:1 chat
        private async Task OnMessageActivityInPersonalChatAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null))
            {
                this.telemetryClient.TrackTrace("Card submit in 1:1 chat");
                await this.OnAdaptiveCardSubmitInPersonalChatAsync(message, turnContext, cancellationToken);
                return;
            }

            string text = (message.Text ?? string.Empty).Trim().ToLower();

            switch (text)
            {
                case AskAnExpert:
                    this.telemetryClient.TrackTrace("Sending user ask an expert card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard()));
                    break;

                case Feedback:
                    this.telemetryClient.TrackTrace("Sending user feedback card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard()));
                    break;

                case TakeATour:
                    this.telemetryClient.TrackTrace("Sending user tour card");
                    var tourCardCarouselAttachment = this.CreateUserTourCardCarouselAttachment();
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(tourCardCarouselAttachment));
                    break;

                default:
                    this.telemetryClient.TrackTrace("Sending input to QnAMaker");
                    var queryResult = await this.GetAnswerFromQnAMakerAsync(text, turnContext, cancellationToken);
                    if (queryResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseAdaptiveCard.GetCard(queryResult.Questions[0], queryResult.Answer, text)));
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInput.GetCard(text)));
                    }

                    break;
            }
        }

        // Handles message activity in channel
        private async Task OnMessageActivityInChannelAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null))
            {
                this.telemetryClient.TrackTrace("Card submit in channel");
                await this.OnAdaptiveCardSubmitInChannelAsync(message, turnContext, cancellationToken);
                return;
            }

            string text = (message.Text ?? string.Empty).Trim().ToLower();

            switch (text)
            {
                case TeamTour:
                    this.telemetryClient.TrackTrace("Sending team tour card");
                    var teamTourCards = this.CreateTeamTourCardCarouselAttachment();
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(teamTourCards));
                    break;

                default:
                    this.telemetryClient.TrackTrace("Unrecognized input in channel");
                    var unrecognizedInputCard = UnrecognizedTeamInput.GetCard();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(unrecognizedInputCard));
                    break;
            }
        }

        // Handles adaptive card submit in 1:1 chat
        // Submits the question or feedback to the SME team
        private async Task OnAdaptiveCardSubmitInPersonalChatAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var payload = ((JObject)message.Value).ToObject<UserActivity>();

            if (!await UserInputValidations.Validate(payload, turnContext, cancellationToken))
            {
                return;
            }

            Attachment smeTeamCard = null;      // Notification to SME team
            Attachment userCard = null;         // Acknowledgement to the user

            var channelAccountDetails = await this.GetPersonalChatUserAccountDetailsAsync(turnContext, cancellationToken);
            var fullName = turnContext.Activity.From.Name;

            switch (message.Text)
            {
                case QuestionForExpert:
                    // TODO: Create the ticket
                    this.telemetryClient.TrackTrace($"Received question for expert");
                    smeTeamCard = this.GetQuestionForExpertAttachment(channelAccountDetails, payload, fullName);
                    userCard = NotificationCard.GetCard(payload.QuestionForExpert, payload.QuestionUserTitleText);
                    break;

                case AppFeedback:
                    this.telemetryClient.TrackTrace($"Received general app feedback");
                    smeTeamCard = this.GetAppFeedbackAttachment(channelAccountDetails, payload, fullName);
                    userCard = ThankYouAdaptiveCard.GetCard();
                    break;

                case ResultsFeedback:
                    this.telemetryClient.TrackTrace($"Received feedback about an answer");
                    smeTeamCard = this.GetResultsFeedbackAttachment(channelAccountDetails, payload, fullName);
                    userCard = ThankYouAdaptiveCard.GetCard();
                    break;

                default:
                    this.telemetryClient.TrackTrace($"Unexpected text in submit payload: {message.Text}", SeverityLevel.Warning);
                    break;
            }

            // Send message to SME team
            if (smeTeamCard != null)
            {
                var channelId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
                await this.SendMessageToTeamAsync(turnContext, smeTeamCard, channelId, cancellationToken);
            }

            // Send acknowledgement to the user
            if (userCard != null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userCard), cancellationToken);
            }
        }

        // Handles adaptive card submit in channel
        private async Task OnAdaptiveCardSubmitInChannelAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // TODO: Handle ticket lifecycle (rename this method as needed)
            await turnContext.SendActivityAsync(MessageFactory.Text("Not yet implemented"));
        }

        // Get an answer from QnAMaker
        private async Task<QueryResult> GetAnswerFromQnAMakerAsync(string message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            try
            {
                var kbId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId);
                if (string.IsNullOrEmpty(kbId))
                {
                    this.telemetryClient.TrackTrace("Knowledge base ID was not found in configuration", SeverityLevel.Warning);
                    return null;
                }

                var endpointKey = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.QnAMakerEndpointKey);
                if (string.IsNullOrEmpty(endpointKey))
                {
                    this.telemetryClient.TrackTrace("QnAMaker endpoint key was not found in configuration", SeverityLevel.Warning);
                    return null;
                }

                var qnaMaker = this.qnaMakerFactory.GetQnAMaker(kbId);
                var options = new QnAMakerOptions { Top = Top, ScoreThreshold = float.Parse(this.configuration["ScoreThreshold"]) };

                var response = await qnaMaker.GetAnswersAsync(turnContext, options);
                return response?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Per spec, treat errors getting a response from QnAMaker as if we got no results from QnAMaker
                this.telemetryClient.TrackTrace($"Error getting answer from QnAMaker, will convert to no result: {ex.Message}");
                this.telemetryClient.TrackException(ex);
                return null;
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
        private async Task<ConversationResourceResponse> SendMessageToTeamAsync(ITurnContext turnContext, Attachment attachmentToSend, string teamId, CancellationToken cancellationToken)
        {
            var conversationParameters = new ConversationParameters
            {
                Activity = (Activity)MessageFactory.Attachment(attachmentToSend),
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamId) },
            };

            var tcs = new TaskCompletionSource<ConversationResourceResponse>();
            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                null,       // If we set channel = "msteams", there is an error as preinstalled middleware expects ChannelData to be present
                turnContext.Activity.ServiceUrl,
                new Bot.Connector.Authentication.MicrosoftAppCredentials(this.configuration["MicrosoftAppId"], this.configuration["MicrosoftAppPassword"]),
                conversationParameters,
                (newTurnContext, newCancellationToken) =>
                {
                    var activity = newTurnContext.Activity;
                    tcs.SetResult(new ConversationResourceResponse
                    {
                        Id = activity.Conversation.Id,
                        ActivityId = activity.Id,
                        ServiceUrl = activity.ServiceUrl,
                    });
                    return Task.CompletedTask;
                },
                cancellationToken);

            return await tcs.Task;
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
    }
}