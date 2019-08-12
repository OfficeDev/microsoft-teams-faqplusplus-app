// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a feedback through bot menu or from response card.
    /// </summary>
    public class SmeFeedbackCard
    {
        /// <summary>
        /// This method will construct the card for SME team which will have the
        /// feedback details given by the user.
        /// </summary>
        /// <param name="payload">user activity payload</param>
        /// <param name="userDetails">User details.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(SubmitUserRequestPayload payload, TeamsChannelAccount userDetails)
        {
            payload.QuestionForExpert = CardHelper.TruncateStringIfLonger(payload.QuestionForExpert, CardHelper.UserDescriptionMaxLength);
            payload.SmeAnswer = CardHelper.TruncateStringIfLonger(payload.SmeAnswer, CardHelper.KbAnswerMaxLength);
            var chatTextButton = string.Format(Resource.ChatTextButton, userDetails.GivenName);

            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard("1.0")
            {
               Body = new List<AdaptiveElement>
               {
                   new AdaptiveFactSet
                   {
                       Facts = new List<AdaptiveFact>
                       {
                           new AdaptiveFact()
                           {
                               Title = Resource.RatingFactTitle,
                               Value = payload.FeedbackRatingAction
                           }
                       },
                   },
                    new AdaptiveTextBlock()
                    {
                        Text = string.Format(Resource.FeedbackAlertText, userDetails.Name, payload.QuestionForExpert),
                        Wrap = true
                    }
               },
               Actions = new List<AdaptiveAction>
               {
                   new AdaptiveOpenUrlAction
                   {
                       Title = chatTextButton,
                       UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userDetails.UserPrincipalName)}"
                   }
               }
            };

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(payload.QuestionForExpert))
            {
                smeFeedbackCard.Body.Add(new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact()
                        {
                            Title = Resource.DescriptionFact,
                            Value = payload.QuestionForExpert
                        },
                    }
                });
            }

            // Question asked fact and view article show card is available when feedback is on QnA Maker response.
            if (!string.IsNullOrWhiteSpace(payload.SmeAnswer) && !string.IsNullOrWhiteSpace(payload.UserQuestion))
            {
                smeFeedbackCard.Body.Add(new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact()
                        {
                            Title = Resource.QuestionAskedFactTitle,
                            Value = payload.UserQuestion
                        },
                    }
                });
                smeFeedbackCard.Actions.AddRange(new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        Title = Resource.ViewArticleButtonText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Body = new List<AdaptiveElement>
                            {
                               new AdaptiveTextBlock
                               {
                                   Text = payload.SmeAnswer,
                                   Wrap = true
                               }
                            }
                        }
                    }
                });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = smeFeedbackCard,
            };
        }
    }
}