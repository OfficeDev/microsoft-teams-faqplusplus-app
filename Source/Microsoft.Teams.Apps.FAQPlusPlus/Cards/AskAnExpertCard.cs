﻿// <copyright file="AskAnExpertCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Ask An Expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public class AskAnExpertCard
    {
        /// <summary>
        /// This method will construct the adaptive card for ask an expert bot menu.
        /// </summary>
        /// <returns>Ask an Expert as an Attachment.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard askAnExpertCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.AskAnExpertText1,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Resource.AskAnExpertPlaceholderText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.TitleText,
                        Wrap = true
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Resource.MandatoryFieldText,
                        Color = AdaptiveTextColor.Attention,
                        Spacing = AdaptiveSpacing.Small,
                        Wrap = true
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(SubmitUserRequestPayload.QuestionUserTitleText),
                        Placeholder = Resource.ShowCardTitleText,
                        IsMultiline = false
                    },
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.DescriptionText,
                        Wrap = true
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(SubmitUserRequestPayload.QuestionForExpert),
                        Placeholder = Resource.AskAnExpertPlaceholderText,
                        IsMultiline = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.AskAnExpertButtonText,
                        Data = new
                        {
                            msteams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.AskAnExpertDisplayText,
                                Text = SubmitUserRequestPayload.QuestionForExpertAction
                            }
                        },
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = askAnExpertCard,
            };
        }
    }
}