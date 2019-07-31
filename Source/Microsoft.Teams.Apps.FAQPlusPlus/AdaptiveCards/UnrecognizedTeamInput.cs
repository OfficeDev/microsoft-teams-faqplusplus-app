﻿// <copyright file="UnrecognizedTeamInput.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the team member-sending random text to bot.
    /// </summary>
    public class UnrecognizedTeamInput
    {
        /// <summary>
        /// Construct the card to render when there's an unrecognized input in a channel.
        /// </summary>
        /// <returns>Card attachment</returns>
        public static Attachment GetCard()
        {
            var card = new HeroCard
            {
                Text = Resource.TeamCustomMessage,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack)
                    {
                        Title = Resource.TakeATeamTourButtonText,
                        DisplayText = Resource.TakeATeamTourButtonText,
                        Text = "team tour",
                    }
                }
            };
            return card.ToAttachment();
        }
    }
}
