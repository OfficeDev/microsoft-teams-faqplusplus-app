// <copyright file="Startup.Auth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using global::Owin;

    /// <summary>
    /// Startup file
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configure Auth
        /// </summary>
        /// <param name="app">App builder</param>
        /// <param name="container">DI container</param>
        public void ConfigureAuth(IAppBuilder app, Autofac.IContainer container)
        {
        }
    }
}