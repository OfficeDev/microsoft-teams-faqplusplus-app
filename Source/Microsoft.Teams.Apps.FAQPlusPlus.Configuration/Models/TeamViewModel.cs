// <copyright file="TeamViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Team View Model
    /// </summary>
    public class TeamViewModel
    {
        /// <summary>
        /// Gets or sets Team Id to be used for Data annotation
        /// </summary>
        [Required(ErrorMessage ="Team Id is required")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name ="Team Id")]
        [RegularExpression(@"(\S)+", ErrorMessage = "White space is not allowed")]
        public string TeamId { get; set; }
    }
}