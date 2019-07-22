// <copyright file="QnAMakerService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;

    /// <summary>
    /// Helper for accessing QnA Maker APIs
    /// </summary>
    public class QnAMakerService
    {
        private const string QnAMakerEndPoint = "https://westus.api.cognitive.microsoft.com";
        private readonly string subscriptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerService"/> class.
        /// </summary>
        /// <param name="subscriptionKey">QnA Maker subscription key</param>
        public QnAMakerService(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Gets Knowledge base details.
        /// </summary>
        /// <param name="kbId">knowledge base id</param>
        /// <returns>Task that resolves to <see cref="GetKnowledgeBaseIdAsync"/>.</returns>
        public async Task<string> GetKnowledgeBaseIdAsync(string kbId)
        {
            try
            {
                var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(this.subscriptionKey)) { Endpoint = QnAMakerEndPoint };
                KnowledgebaseDTO kbFromQnAMakerService = await client.Knowledgebase.GetDetailsAsync(kbId);

                return kbFromQnAMakerService.Id;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
