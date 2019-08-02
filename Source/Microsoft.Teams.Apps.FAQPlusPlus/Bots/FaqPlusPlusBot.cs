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
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.Validations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements the core logic of the FAQ++ bot.
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

        private readonly TelemetryClient telemetryClient;
        private readonly IConfigurationProvider configurationProvider;
        private readonly MessagingExtension messageExtension;
        private readonly IQnAMakerFactory qnaMakerFactory;
        private readonly string appBaseUri;
        private readonly MicrosoftAppCredentials microsoftAppCredentials;
        private readonly ITicketsProvider ticketsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="telemetryClient"> Telemetry Client.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="qnaMakerFactory">QnAMaker factory instance</param>
        /// <param name="messageExtension">Messaging extension instance</param>
        /// <param name="appBaseUri">Base URI at which the app is served</param>
        /// <param name="microsoftAppCredentials">Microsoft app credentials to use</param>
        /// <param name="ticketsProvider">The tickets provider.</param>
        public FaqPlusPlusBot(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            IQnAMakerFactory qnaMakerFactory,
            MessagingExtension messageExtension,
            string appBaseUri,
            MicrosoftAppCredentials microsoftAppCredentials,
            ITicketsProvider ticketsProvider)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.qnaMakerFactory = qnaMakerFactory;
            this.messageExtension = messageExtension;
            this.appBaseUri = appBaseUri;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.ticketsProvider = ticketsProvider;
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

                await this.SendTypingIndicatorAsync(turnContext);

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

                this.telemetryClient.TrackTrace($"Received conversationUpdate activity");
                this.telemetryClient.TrackTrace($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count()}, membersRemoved: {activity.MembersRemoved?.Count()}");

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
                await this.SendCardToTeamAsync(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken);
            }
        }

        // Handles message activity in 1:1 chat
        private async Task OnMessageActivityInPersonalChatAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
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
                    var userTourCards = TourCarousel.GetUserTourCards(this.appBaseUri);
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(userTourCards));
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
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
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
                    var teamTourCards = TourCarousel.GetTeamTourCards(this.appBaseUri);
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
            TicketEntity newTicket = null;      // New ticket

            var userDetails = await this.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken);

            switch (message.Text)
            {
                case QuestionForExpert:
                    this.telemetryClient.TrackTrace($"Received question for expert");

                    newTicket = await this.CreateTicketAsync(message, payload, userDetails);

                    smeTeamCard = IncomingSMEEnquiryCard.CreateTicketCard(payload.QuestionUserTitleText, userDetails, payload, newTicket.TicketId);
                    userCard = NotificationCard.GetCard(payload.QuestionForExpert, payload.QuestionUserTitleText);
                    break;

                case AppFeedback:
                    this.telemetryClient.TrackTrace($"Received general app feedback");
                    smeTeamCard = IncomingSMEEnquiryCard.CreateAppFeedbackCard(payload.FeedbackUserTitleText, userDetails, payload);
                    userCard = ThankYouAdaptiveCard.GetCard();
                    break;

                case ResultsFeedback:
                    this.telemetryClient.TrackTrace($"Received feedback about an answer");
                    smeTeamCard = IncomingSMEEnquiryCard.CreateResultFeedbackCard(payload.FeedbackUserTitleText, userDetails, payload);
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
                var resourceResponse = await this.SendCardToTeamAsync(turnContext, smeTeamCard, channelId, cancellationToken);

                // If a ticket was created, update the ticket with the conversation info
                if (newTicket != null)
                {
                    newTicket.SmeCardActivityId = resourceResponse.ActivityId;
                    newTicket.SmeThreadConversationId = resourceResponse.Id;
                    await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(newTicket);
                }
            }

            // Send acknowledgment to the user
            if (userCard != null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userCard), cancellationToken);
            }
        }

        // Handles adaptive card submit in channel
        private async Task OnAdaptiveCardSubmitInChannelAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var payload = ((JObject)message.Value).ToObject<TicketDetails>();

            var ticket = await this.ticketsProvider.GetSavedTicketEntityDetailAsync(payload.RowKey);

            // Update the tickets based on the payload
            switch (payload.Status)
            {
                case "0": // Open
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = null;
                    ticket.AssignedToName = null;
                    ticket.AssignedToObjectId = null;
                    ticket.DateClosed = null;
                    break;

                case "1": // Close
                    ticket.Status = (int)TicketState.Closed;
                    ticket.DateClosed = DateTime.UtcNow;
                    break;

                case "2": // Assign to self
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = DateTime.UtcNow;
                    ticket.AssignedToName = message.From.Name;
                    ticket.AssignedToObjectId = message.From.AadObjectId;
                    ticket.DateClosed = null;
                    break;

                default:
                    // TODO: Show error message
                    this.telemetryClient.TrackTrace($"Unknown status command {payload.Status}", SeverityLevel.Warning);
                    return;
            }

            ticket.LastModifiedByName = message.From.Name;
            ticket.LastModifiedByObjectId = message.From.AadObjectId;

            await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(ticket);

            // Update the card in the SME team
            var updateCardActivity = new Activity(ActivityTypes.Message)
            {
                Id = ticket.SmeCardActivityId,
                Conversation = new ConversationAccount { Id = ticket.SmeThreadConversationId },
                Attachments = new List<Attachment> { new SmeTicketCard(ticket).ToAttachment() },
            };
            await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);

            // Post update to user and SME team thread
            IMessageActivity userNotification = null;
            switch (payload.Status)
            {
                case "0": // Open
                    await turnContext.SendActivityAsync(string.Format(Resource.SMEOpenedStatus, turnContext.Activity.From.Name));
                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.ReopenedTicketUserNotification));
                    break;

                case "1": // Close
                    await turnContext.SendActivityAsync(string.Format(Resource.SMEClosedStatus, ticket.LastModifiedByName));
                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.ClosedTicketUserNotification));
                    break;

                case "2": // Assign to self
                    await turnContext.SendActivityAsync(string.Format(Resource.SMEAssignedStatus, ticket.AssignedToName));
                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.AssignedTicketUserNotification));
                    break;
            }

            if (userNotification != null)
            {
                userNotification.Conversation = new ConversationAccount { Id = ticket.RequesterConversationId };
                await turnContext.Adapter.SendActivitiesAsync(turnContext, new Activity[] { (Activity)userNotification }, cancellationToken);
            }
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
                    this.telemetryClient.TrackTrace("Knowledge base ID was not found in configuration table", SeverityLevel.Warning);
                    return null;
                }

                var endpointKey = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.QnAMakerEndpointKey);
                if (string.IsNullOrEmpty(endpointKey))
                {
                    this.telemetryClient.TrackTrace("QnAMaker endpoint key was not found in configuration table", SeverityLevel.Warning);
                    return null;
                }

                var qnaMaker = this.qnaMakerFactory.GetQnAMaker(kbId, endpointKey);
                var response = await qnaMaker.GetAnswersAsync(turnContext);
                return response?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Per spec, treat errors getting a response from QnAMaker as if we got no results
                this.telemetryClient.TrackTrace($"Error getting answer from QnAMaker, will convert to no result: {ex.Message}");
                this.telemetryClient.TrackException(ex);
                return null;
            }
        }

        /// <summary>
        /// Handle invoke activities received by the bot.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
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
        /// Get the account details of the user in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> Team channel account details.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        /// <summary>
        /// Send typing indicator to the user.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        private Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            return turnContext.SendActivityAsync(typingActivity);
        }

        /// <summary>
        /// Send the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cardToSend">The card to send.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> that resolves to a <see cref="ConversationResourceResponse"/></returns>
        private async Task<ConversationResourceResponse> SendCardToTeamAsync(ITurnContext turnContext, Attachment cardToSend, string teamId, CancellationToken cancellationToken)
        {
            var conversationParameters = new ConversationParameters
            {
                Activity = (Activity)MessageFactory.Attachment(cardToSend),
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamId) },
            };

            var tcs = new TaskCompletionSource<ConversationResourceResponse>();
            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                null,       // If we set channel = "msteams", there is an error as preinstalled middleware expects ChannelData to be present
                turnContext.Activity.ServiceUrl,
                this.microsoftAppCredentials,
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

        // Create a new ticket from the input
        private async Task<TicketEntity> CreateTicketAsync(IMessageActivity message, UserActivity payload, TeamsChannelAccount member)
        {
            TicketEntity ticketEntity = new TicketEntity
            {
                TicketId = Guid.NewGuid().ToString(),
                Status = (int)TicketState.Open,
                DateCreated = DateTime.UtcNow,
                Title = payload.QuestionUserTitleText,
                Description = payload.QuestionForExpert,
                RequesterName = member.Name,
                RequesterUserPrincipalName = member.UserPrincipalName,
                RequesterGivenName = member.GivenName,
                RequesterConversationId = message.Conversation.Id,
                LastModifiedByName = message.From.Name,
                LastModifiedByObjectId = message.From.AadObjectId,
                UserQuestion = payload.UserQuestion,
                KnowledgeBaseAnswer = payload.SmeAnswer
            };

            await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(ticketEntity);

            return ticketEntity;
        }

        /// <summary>
        /// Updates the SME activity card in place.
        /// </summary>
        /// <param name="ticket">The ticket for which the information should be shown.</param>
        /// <param name="attachmentToSend">The card which will update the current card in the conversation.</param>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private async Task UpdateSMEEnquiryCard(
           TicketEntity ticket,
           Attachment attachmentToSend,
           ITurnContext<IMessageActivity> turnContext,
           CancellationToken cancellationToken)
        {
            var updateCardActivity = new Activity()
            {
                Id = ticket.SmeCardActivityId,
                Conversation = new ConversationAccount()
                {
                    Id = ticket.SmeThreadConversationId,
                },
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment>
                {
                    attachmentToSend,
                },
            };

            await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);
        }

        /// <summary>
        /// Method which will inform the user of the updates in the ticket.
        /// </summary>
        /// <param name="conversationId">The original conversationId that posted the request.</param>
        /// <param name="messageToSend">The text to send back to the user (status update).</param>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private async Task InformUserOfUpdates(
            string conversationId,
            string messageToSend,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var userMessageActivity = new Activity()
            {
                Conversation = new ConversationAccount()
                {
                    Id = conversationId,
                },
                Type = ActivityTypes.Message,
                Text = messageToSend,
            };

            await ((BotFrameworkAdapter)turnContext.Adapter).SendActivitiesAsync(turnContext, new Activity[] { userMessageActivity }, cancellationToken);
        }

        /// <summary>
        /// Updates the newly created ticket record with the right information about the conversation.
        /// </summary>
        /// <param name="ticketId">The ticketId to update.</param>
        /// <param name="activityId">The correct activityId.</param>
        /// <param name="threadConversationId">The conversationId in the SME channel.</param>
        /// <returns>A unit of execution.</returns>
        private async Task UpdateConversationInfo(string ticketId, string activityId, string threadConversationId)
        {
            var tableResult = await this.ticketsProvider.GetSavedTicketEntityDetailAsync(ticketId);
            var ticketEntity = tableResult;
            ticketEntity.SmeCardActivityId = activityId;
            ticketEntity.SmeThreadConversationId = threadConversationId;
            await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(ticketEntity);
        }

        /// <summary>
        /// Gets the date/time for the current ticket.
        /// </summary>
        /// <param name="ticketDetails">The ticket details.</param>
        /// <returns>A nullable object.</returns>
        private DateTime? SetDateTime(TicketDetails ticketDetails)
        {
            if (ticketDetails.Status == "0")
            {
                return null;
            }
            else
            {
                return DateTime.UtcNow;
            }
        }

        private DateTime? SetClosedDateTime(TicketDetails ticketDetails)
        {
            if (ticketDetails.Status == "1")
            {
                return DateTime.UtcNow;
            }
            else
            {
                return null;
            }
        }
    }
}