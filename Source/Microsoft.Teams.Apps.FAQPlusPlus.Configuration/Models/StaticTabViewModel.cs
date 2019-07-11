﻿// <copyright file="StaticTabViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Static tab view model
    /// </summary>
    public class StaticTabViewModel
    {
        /// <summary>
        /// Gets or sets static tab message text box to be used in View
        /// </summary>
        [Required(ErrorMessage = "Enter a static tab text.")]
        [StringLength(maximumLength: 300, ErrorMessage = "Enter static tab text which should contain less than 300 characters.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Static tab text")]
        public string StaticTabText { get; set; }
    }
}