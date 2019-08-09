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
        /// Method to generate the adaptive card.
        /// </summary>
        /// <param name="payload">user activity payload</param>
        /// <param name="userDetails">User details.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(SubmitUserRequestPayload payload, TeamsChannelAccount userDetails)
        {
            payload.QuestionForExpert = CardHelper.TruncateStringIfLonger(payload.QuestionForExpert, CardHelper.DescriptionText);

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
                             new AdaptiveFact
                             {
                                Title = Resource.RatingFactTitle,
                                Value = payload.FeedbackRatingAction
                             },
                             new AdaptiveFact
                             {
                                Title = Resource.DescriptionText,
                                Value = string.Format(Resource.FeedbackSubHeaderText, userDetails.Name, payload.QuestionForExpert)
                             }
                         },
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

            if (!string.IsNullOrEmpty(payload.UserQuestion) && !string.IsNullOrEmpty(payload.SmeAnswer))
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