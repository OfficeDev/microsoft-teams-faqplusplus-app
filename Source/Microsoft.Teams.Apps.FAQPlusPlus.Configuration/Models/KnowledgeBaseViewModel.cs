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
        /// Gets or sets knowledge base Id to be used in View
        /// </summary>
        [Required(ErrorMessage = "Knowledge base Id is required")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Knowledge base Id")]
        [RegularExpression(@"(\S)+", ErrorMessage = "White space is not allowed")]
        public string KnowledgeBaseIdTextBox { get; set; }
    }
}