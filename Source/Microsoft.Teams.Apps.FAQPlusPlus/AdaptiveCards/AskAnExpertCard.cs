// <copyright file="AskAnExpertCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.BotHelperMethods.AdaptiveCards
{
    using System.Collections.Generic;
    using System.IO;
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Ask An Expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class AskAnExpertCard
    {
        /// <summary>
        /// This method will construct the adaptive card as an Attachment using JSON template.
        /// </summary>
        /// <returns>Ask an Expert as an Attachment.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard askAnExpertCard = new AdaptiveCard("1.0");
            askAnExpertCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.AskAnExpertText1,
                Wrap = true
            });

            askAnExpertCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.AskAnExpertPlaceholderText,
                Wrap = true
            });

            askAnExpertCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.TitleText,
                Wrap = true
            });

            askAnExpertCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
                Text = Resource.MandatoryFieldText,
                Color = AdaptiveTextColor.Attention,
                Spacing = AdaptiveSpacing.Small,
                Wrap = true
            });

            askAnExpertCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "questionUserTitleText",
                Placeholder = Resource.ShowCardTitleText,
                IsMultiline = false
            });

            askAnExpertCard.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Bolder,
                Text = Resource.DescriptionText,
                Wrap = true
            });

            askAnExpertCard.Body.Add(new AdaptiveTextInput()
            {
                Id = "questionForExpert",
                Placeholder = Resource.AskAnExpertPlaceholderText,
                IsMultiline = true
            });

            askAnExpertCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resource.AskAnExpertButtonText,
                Data = Newtonsoft.Json.Linq.JObject.FromObject(
                new
                {
                    msteams = new
                    {
                        type = "messageBack",
                        displayText = Resource.AskAnExpertDisplayText,
                        text = "QuestionForExpert"
                    }
                })
            });
            return CardHelper.GenerateCardAttachment(askAnExpertCard.ToJson());
        }
    }
}