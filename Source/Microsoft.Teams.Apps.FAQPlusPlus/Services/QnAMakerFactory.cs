// <copyright file="QnAMakerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using System.Collections.Concurrent;
    using System.Net.Http;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Produces the right <see cref="QnAMaker"/> instance for a knowledge base.
    /// </summary>
    public class QnAMakerFactory : IQnAMakerFactory
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly ConcurrentDictionary<string, QnAMaker> qnaMakerInstances;

        public QnAMakerFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.httpClient = new HttpClient();
            this.qnaMakerInstances = new ConcurrentDictionary<string, QnAMaker>();
        }

        /// <inheritdoc/>
        public QnAMaker GetQnAMaker(string knowledgeBaseId)
        {
            return this.qnaMakerInstances.GetOrAdd(knowledgeBaseId, (kbId) =>
                new QnAMaker(
                    new QnAMakerService
                    {
                        KbId = kbId,
                        EndpointKey = this.configuration["EndpointKey"],
                        Hostname = this.configuration["KbHost"]
                    },
                    null,
                    this.httpClient));
        }
    }
}
