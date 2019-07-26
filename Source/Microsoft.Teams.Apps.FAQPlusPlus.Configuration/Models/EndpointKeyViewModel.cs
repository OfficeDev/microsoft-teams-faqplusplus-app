// <copyright file="EndpointKeyViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Endpoint key View Model
    /// </summary>
    public class EndpointKeyViewModel
    {
        /// <summary>
        /// Gets or sets Endpoint key text box to be used in View
        /// </summary>
        [Required(ErrorMessage = "Enter Endpoint key.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Endpoint key")]
        [RegularExpression(@"(\S)+", ErrorMessage = "Enter Endpoint key which should not contain any whitespace.")]
        public string EndpointKey { get; set; }
    }
}