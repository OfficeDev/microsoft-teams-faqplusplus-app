// <copyright file="UnrecognizedTeamInput.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the team member-sending random text to bot.
    /// </summary>
    public class UnrecognizedTeamInput
    {

        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard unrecognizedTeamInputCard = new AdaptiveCard("1.0");
            unrecognizedTeamInputCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = Resource.TeamCustomMessage,
                Wrap = true
            });

            // Team tour submit button
            unrecognizedTeamInputCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resource.TakeATeamTourButtonText,
                Data = Newtonsoft.Json.Linq.JObject.FromObject(
                     new
                     {
                         msteams = new
                         {
                             type = "messageBack",
                             displayText = Resource.TakeATeamTourButtonText,
                             text = "team tour"
                         }
                     })
            });

            return CardHelper.GenerateCardAttachment(unrecognizedTeamInputCard.ToJson());
        }
    }
}
