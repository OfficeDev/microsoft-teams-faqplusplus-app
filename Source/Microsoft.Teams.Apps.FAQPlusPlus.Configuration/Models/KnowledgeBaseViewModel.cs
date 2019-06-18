// <copyright file="KnowledgeBaseViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// KnowledgeBase View Model
    /// </summary>
    public class KnowledgeBaseViewModel
    {
        /// <summary>
        /// Gets or sets knowledge base Url text box
        /// </summary>
        [Required(ErrorMessage = "Knowledge base URL is required")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Knowledge base URL")]
        [RegularExpression(@"(\S)+", ErrorMessage = "White space is not allowed")]
        public string KnowledgeBaseUrlTextBox { get; set; }
    }
}