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
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.Validations;
    using Newtonsoft.Json;
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
        private readonly IQnAMakerFactory qnaMakerFactory;
        private readonly ITicketsProvider ticketsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="telemetryClient"> Telemetry Client.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="qnaMakerFactory">The QnAMaker Factory - repository for all the QnAMaker calls.</param>
        /// <param name="ticketsProvider">The repository for all the calls to the database.</param>
        public FaqPlusPlusBot(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            IConfiguration configuration,
            IQnAMakerFactory qnaMakerFactory,
            ITicketsProvider ticketsProvider)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.configuration = configuration;
            this.qnaMakerFactory = qnaMakerFactory;
            this.ticketsProvider = ticketsProvider;
        }

        /// <summary>
        /// Sends update to the user in adaptive cards, after bot posting user query to SME channel.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="updateActivityAttachment">Activity update adaptive card attachment.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Thank you Card.<see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendInfoReceievedConfirmation(ITurnContext turnContext, Attachment updateActivityAttachment, CancellationToken cancellationToken)
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
            var qnaMaker = this.qnaMakerFactory.GetQnAMaker(kbId);
            var options = new QnAMakerOptions { Top = Top, ScoreThreshold = float.Parse(this.configuration["ScoreThreshold"]) };
            var response = await qnaMaker.GetAnswersAsync(turnContext, options);
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
        /// <param name="payload">The user activity object.</param>
        /// <param name="channelAccountDetails">The channel details.</param>
        /// <param name="ticketId">The newly created ticketId.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Notification to SME team channel.<see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ForwardInformationToTeam(
            ITurnContext<IMessageActivity> turnContext,
            UserActivity payload,
            TeamsChannelAccount channelAccountDetails,
            string ticketId,
            CancellationToken cancellationToken)
        {
            var fullName = turnContext.Activity.Recipient.Name;
            Attachment teamCardAttachment = null;
            string activityType = string.IsNullOrEmpty(turnContext.Activity.Text) ? string.Empty : turnContext.Activity.Text.Trim().ToLower();
            switch (activityType)
            {
                case AppFeedback:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("App Feedback", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.AppFeedback);
                    break;

                case QuestionForExpert:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("Question For Expert", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.QuestionForExpert, string.Empty, string.Empty, ticketId);
                    break;

                case ResultsFeedback:
                    teamCardAttachment = IncomingSMEEnquiryCard.GetCard("Results Feedback", fullName, channelAccountDetails.GivenName, channelAccountDetails.Email, payload.ResultsFeedback, payload.SMEQuestion, payload.SMEAnswer);
                    break;

                default:
                    break;
            }

            await this.DisplayTypingIndicator(turnContext);

            if (payload.QuestionForExpert != null)
            {
                var confirmationAttachment = ThankYouAdaptiveCard.GetCard();

                // Create the conversationId and activity
                var bot = new ChannelAccount { Id = turnContext.Activity.Recipient.Id };
                var conversationParameters = new ConversationParameters()
                {
                    Bot = bot,
                    ChannelData = new TeamsChannelData()
                    {
                        Channel = new ChannelInfo(this.configuration["ChannelId"]),
                    },
                    IsGroup = true,
                    Activity = new Activity()
                    {
                        Type = ActivityTypes.Message,
                        Attachments = new List<Attachment>()
                        {
                            teamCardAttachment,
                        },
                    },
                };

                try
                {
                    await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(this.configuration["ChannelId"], turnContext.Activity.ServiceUrl, new Bot.Connector.Authentication.MicrosoftAppCredentials(this.configuration["MicrosoftAppId"], this.configuration["MicrosoftAppPassword"]), conversationParameters, (turnCtx, canToken) =>
                    {
                        var activityId = turnCtx.Activity.Id;
                        var conversationId = turnCtx.Activity.Conversation.Id;
                        _ = this.SendInfoReceievedConfirmation(turnContext, confirmationAttachment, cancellationToken);
                        _ = this.UpdateConversationInfo(ticketId, activityId, conversationId);
                        return null;
                    },
                    cancellationToken);
                }
                catch (Exception ex)
                {
                    this.telemetryClient.TrackException(ex);
                }
            }
            else
            {
                await this.SendInfoReceievedConfirmation(turnContext, ThankYouAdaptiveCard.GetCard(), cancellationToken);
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

        public async Task UpdateTableEntityValues(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var ticketDetails = JsonConvert.DeserializeObject<TicketDetails>(turnContext.Activity.Value.ToString());
            var tableResult = await this.ticketsProvider.GetSavedTicketEntityDetailAsync(ticketDetails.RowKey);
            var ticketEntity = tableResult;
            ticketEntity.Status = Convert.ToInt16(ticketDetails.Status);
            ticketEntity.AssignedTo = turnContext.Activity.From.Name;
            ticketEntity.DateAssigned = DateTime.UtcNow;
            ticketEntity.AssignedToObjectId = turnContext.Activity.From.AadObjectId;
            await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(ticketEntity);
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

                // var response = client.Conversations.CreateOrGetDirectConversation(activity.Recipient, activity.From, activity.GetTenantId());
                var data = turnContext.Activity.GetConversationReference();

                var conversationType = turnContext.Activity.Conversation.ConversationType;

                // when conversation is from Teams channel
                if (conversationType == "channel")
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
                        // TODO: Send cards to the user
                        // await this.SendCardsUsrAsync(turnContext, cancellationToken);
                        await this.UpdateTableEntityValues(turnContext, cancellationToken);

                        // TODO: Reply to the card and update card in place
                        var ticketDetails = JsonConvert.DeserializeObject<TicketDetails>(turnContext.Activity.Value.ToString());
                        var tableResult = await this.ticketsProvider.GetSavedTicketEntityDetailAsync(ticketDetails.RowKey);

                        var updateActivityMessage = string.Empty;
                        var conversationUpdateMessage = string.Empty;

                        if (tableResult.Status == 2)
                        {
                            updateActivityMessage = string.Format(Resource.SMEAssignedStatus, tableResult.AssignedTo);
                        }
                        else if (tableResult.Status == 1)
                        {
                            updateActivityMessage = string.Format(Resource.SMEOpenedStatus, tableResult.AssignedTo);
                        }
                        else if (tableResult.Status == 0)
                        {
                            updateActivityMessage = string.Format(Resource.SMEClosedStatus, tableResult.AssignedTo);
                        }

                        await this.UpdateAuditTrail(tableResult.CardActivityId, updateActivityMessage, turnContext, cancellationToken);
                        await this.UpdateSMEEnquiryCard(tableResult.CardActivityId, tableResult.ThreadConversationId, turnContext, cancellationToken);

                        // await this.NotifyTeam(turnContext, ConfirmationCard.GetCard(), this.configuration["ChannelId"], cancellationToken);
                    }
                    else
                    {
                        var teamtourCardAttachment = UnrecognizedTeamInput.GetCard();
                        await this.SendTeamMessage(turnContext, teamtourCardAttachment, cancellationToken);
                    }
                }
                else if (turnContext.Activity.Value != null && ((JObject)turnContext.Activity.Value).Count != 0 && conversationType == "personal")
                {
                    await this.SendCardsToSMEAsync(turnContext, cancellationToken);
                }
                else if (!string.IsNullOrEmpty(turnContext.Activity.Text))
                {
                    await this.SendCardsAsync(turnContext, cancellationToken);
                }
            }
            catch (StatusUpdateException ex)
            {
                this.telemetryClient.TrackException(ex);
                await turnContext.SendActivityAsync(MessageFactory.Text(ex.Message), cancellationToken);
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
                        var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
                        var userWelcomeCardAttachment = await WelcomeCard.GetCard(welcomeText);
                        this.telemetryClient.TrackTrace($"Member Id of User = {member.Id}");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));
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
        /// Method to create the user ticket entity.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="ticketsProvider">The tickets provider.</param>
        /// <param name="payload">The activity data.</param>
        /// <param name="member">The user.</param>
        /// <returns>A unit of execution that returns a string.</returns>
        private static async Task<string> CreateUserTicketEntity(ITurnContext<IMessageActivity> turnContext, ITicketsProvider ticketsProvider, UserActivity payload, TeamsChannelAccount member)
        {
            var ticketGuid = Guid.NewGuid().ToString();
            TicketEntity ticketEntity = new TicketEntity();
            ticketEntity.OpenedBy = turnContext.Activity.From.Name;
            ticketEntity.Status = (int)TicketState.Open;
            ticketEntity.Text = payload.QuestionForExpert;
            ticketEntity.Timestamp = DateTime.UtcNow;
            ticketEntity.CardActivityId = turnContext.Activity.Id.ToString();
            ticketEntity.RowKey = ticketGuid;
            ticketEntity.TicketId = ticketGuid;
            ticketEntity.DateAssigned = DateTime.UtcNow;
            ticketEntity.DateCreated = DateTime.UtcNow;
            ticketEntity.OpenedByConversationId = turnContext.Activity.Conversation.Id;

            if (await ticketsProvider.SaveOrUpdateTicketEntityAsync(ticketEntity))
            {
                return ticketEntity.RowKey;
            }
            else
            {
                return string.Empty;
            }
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

        /// <summary>
        /// Notification to the SME team when user post a question or feedback to the SME team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="attachmentToSend">sends Adaptive card.</param>
        /// <param name="teamId">Team Id to which the message is being sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Message to the SME Team.<see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task NotifyTeam(ITurnContext turnContext, Attachment attachmentToSend, string teamId, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"There is a snag: {ex.Message}");
                throw;
            }
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
                var payload = ((JObject)turnContext.Activity.Value).ToObject<UserActivity>();
                var channelAccountDetails = this.GetTeamsChannelAccountDetails(turnContext, cancellationToken);
                var ticketId = await CreateUserTicketEntity(turnContext, this.ticketsProvider, payload, channelAccountDetails);
                await this.ForwardInformationToTeam(
                       turnContext,
                       payload,
                       channelAccountDetails,
                       ticketId,
                       cancellationToken);
            }
        }

        /// <summary>
        /// Adds to the audit trail for the card that is coming in for the SME team.
        /// </summary>
        /// <param name="cardActivityId">The CardActivityId to reply to.</param>
        /// <param name="updateActivityMessage">The message to write in the SME team.</param>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private async Task UpdateAuditTrail(
            string cardActivityId,
            string updateActivityMessage,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var replyToCardActivity = new Activity()
            {
                Type = ActivityTypes.Message,
                Text = updateActivityMessage,
            };

            await turnContext.SendActivityAsync(replyToCardActivity, cancellationToken);
        }

        /// <summary>
        /// Updates the SME activity card in place.
        /// </summary>
        /// <param name="cardActivityId">The activityId to replace.</param>
        /// <param name="conversationId">The conversationId reference.</param>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private async Task UpdateSMEEnquiryCard(
            string cardActivityId,
            string conversationId,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var updateCardActivity = new Activity()
            {
                Id = cardActivityId,
                Conversation = new ConversationAccount()
                {
                    Id = conversationId,
                },
                Type = ActivityTypes.Message,
                Text = "Yahtzee!",
            };

            await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);
        }

        private async Task UpdateConversationInfo(string ticketId, string activityId, string threadConversationId)
        {
            var tableResult = await this.ticketsProvider.GetSavedTicketEntityDetailAsync(ticketId);
            var ticketEntity = tableResult;
            ticketEntity.CardActivityId = activityId;
            ticketEntity.ThreadConversationId = threadConversationId;
            await this.ticketsProvider.SaveOrUpdateTicketEntityAsync(ticketEntity);
        }
    }
}