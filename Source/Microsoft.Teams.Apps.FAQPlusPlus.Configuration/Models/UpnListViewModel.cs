// <copyright file="UpnListViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Upn List View Model
    /// </summary>
    public class UpnListViewModel
    {
        /// <summary>
        /// Gets or sets Upn email address text box to be used in View
        /// </summary>
        [Required(ErrorMessage = "UPN's email address is required")]
        [MinLength(10)]
        [DataType(DataType.Text)]
        [Display(Name = "UPN's email address")]
        [EmailAddress]
        public string UpnEmailAddress { get; set; }
    }
}