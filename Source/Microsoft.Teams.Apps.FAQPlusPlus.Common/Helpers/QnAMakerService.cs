// <copyright file="QnAMakerService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper for accessing QnA Maker APIs
    /// </summary>
    public class QnAMakerService : IQnAMakerService
    {
        private const string QnAMakerEndPoint = "https://westus.api.cognitive.microsoft.com";
        private const string MethodKB = "knowledgebases";
        private readonly string subscriptionKey;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerService"/> class.
        /// </summary>
        /// <param name="httpClient">HttpClient for generating http requests</param>
        /// <param name="subscriptionKey">QnA Maker subscription key</param>
        public QnAMakerService(HttpClient httpClient, string subscriptionKey)
        {
            this.httpClient = httpClient;
            this.subscriptionKey = subscriptionKey;
        }

        /// <inheritdoc/>
        public async Task<KnowledgebaseDTO> GetKnowledgeBaseDetailsAsync(string kbId)
        {
            var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(this.subscriptionKey)) { Endpoint = QnAMakerEndPoint };
            return await client.Knowledgebase.GetDetailsAsync(kbId);
        }
    }
}
