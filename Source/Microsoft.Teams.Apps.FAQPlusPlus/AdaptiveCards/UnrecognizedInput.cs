// <copyright file="UnrecognizedInput.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AdaptiveCards
{
    using global::AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the user-asking random question to bot.
    /// </summary>
    public static class UnrecognizedInput
    {
        /// <summary>
        /// This method will construct the adaptive card when unrecognized input is sent by the user.
        /// </summary>
        /// <param name="question">The question that the user asks the bot.</param>
        /// <returns>Card attachment as Json string.</returns>
        public static Attachment GetCard(string question)
        {
            {
                AdaptiveCard unrecognizedInputCard = new AdaptiveCard("1.0");
                unrecognizedInputCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = Resource.CustomMessage,
                    Wrap = true
                });

                // Ask an expert show card
                AdaptiveCard askAnExpertShowCard = new AdaptiveCard("1.0");
                askAnExpertShowCard.Title = Resource.AskAnExpertButtonText;

                askAnExpertShowCard.Body.Add(new AdaptiveTextBlock()
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Text = Resource.TitleText,
                    Wrap = true
                });

                askAnExpertShowCard.Body.Add(new AdaptiveTextBlock()
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Medium,
                    Text = Resource.MandatoryFieldText,
                    Color = AdaptiveTextColor.Attention,
                    Spacing = AdaptiveSpacing.Small,
                    Wrap = true
                });

                askAnExpertShowCard.Body.Add(new AdaptiveTextInput()
                {
                    Placeholder = Resource.ShowCardTitleText,
                    Id = "questionUserTitleText",
                    IsMultiline = false
                });

                askAnExpertShowCard.Body.Add(new AdaptiveTextBlock()
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Text = Resource.DescriptionText,
                    Wrap = true
                });

                askAnExpertShowCard.Body.Add(new AdaptiveTextInput()
                {
                    Id = "questionForExpert",
                    Value = question,
                    IsMultiline = true
                });

                askAnExpertShowCard.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = Resource.SubmitButtonText,
                    Data = Newtonsoft.Json.Linq.JObject.FromObject(
                    new
                    {
                        msteams = new
                        {
                            type = "messageBack",
                            displayText = Resource.AskAnExpertDisplayText,
                            text = "QuestionForExpert"
                        },
                        UserQuestion = question,
                    })
                });

                unrecognizedInputCard.Actions.Add(new AdaptiveShowCardAction()
                {
                    Title = Resource.AskAnExpertButtonText,
                    Card = askAnExpertShowCard
                });
                return CardHelper.GenerateCardAttachment(unrecognizedInputCard.ToJson());
            }
        }
    }
}
