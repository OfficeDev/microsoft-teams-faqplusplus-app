// <copyright file="QnAMakerService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper for accessing QnA Maker APIs
    /// </summary>
    public class QnAMakerService : IQnAMakerService
    {
        private const string QnAMakerRequestUrl = "https://westus.api.cognitive.microsoft.com/qnamaker/v4.0";
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
        public async Task<GetKnowledgeBaseDetailsResponse> GetKnowledgeBaseDetailsAsync(string kbId)
        {
            var uri = $"{QnAMakerRequestUrl}/{MethodKB}/{kbId}";
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequest.Headers.Add(Constants.OcpApimSubscriptionKey, this.subscriptionKey);

                var response = await this.httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<GetKnowledgeBaseDetailsResponse>(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
