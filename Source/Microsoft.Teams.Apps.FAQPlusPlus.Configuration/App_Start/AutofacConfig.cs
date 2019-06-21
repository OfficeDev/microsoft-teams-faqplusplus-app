// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System.Configuration;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers;

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

            builder.Register(c => new TeamHelper(ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<TeamHelper>()
                .SingleInstance();

            builder.Register(c => new HttpClient())
                .SingleInstance();

            builder.Register(c => new KnowledgeBaseHelper(
                 c.Resolve<HttpClient>(),
                 ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"],
                 ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<KnowledgeBaseHelper>()
                .SingleInstance();

            builder.RegisterType<HomeController>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}