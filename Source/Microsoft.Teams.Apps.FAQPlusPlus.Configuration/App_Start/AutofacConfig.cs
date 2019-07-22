// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System.Configuration;
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;

    /// <summary>
    /// Autofac configuration
    /// </summary>
    public class AutofacConfig
    {
        /// <summary>
        /// Register Autofac dependencies
        /// </summary>
        /// <returns>Autofac container</returns>
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(c => new ConfigurationProvider(
                 ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<ConfigurationProvider>()
                .SingleInstance();

            builder.Register(c => new QnAMakerService(
                 ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"]))
                .As<QnAMakerService>()
                .SingleInstance();

            builder.Register(c => new TicketProvider(
                 ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<TicketProvider>()
                .SingleInstance();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}