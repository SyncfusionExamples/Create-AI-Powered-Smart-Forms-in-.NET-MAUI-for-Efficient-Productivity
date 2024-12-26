using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;

namespace AIDataForm
{
    internal class AzureOpenAIBaseService
    {
        #region Fields

        /// <summary>
        /// The Azure OpenAI EndPoint
        /// </summary>
        private const string endpoint = "AZURE_OPENAI_ENDPOINT";

        /// <summary>
        /// The Deployment name
        /// </summary>
        private const string deploymentName = "DEPLOYMENT_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string key = "API_KEY";

        /// <summary>
        /// The OpenAI
        /// </summary>
        private IChatClient? client;

        /// <summary>
        /// The already credential validated field
        /// </summary>
        private bool isAlreadyValidated = false;

        /// <summary>
        /// The chat history
        /// </summary>
        private string? chatHistory;

        #endregion

        internal AzureOpenAIBaseService()
        {
            ValidateCredential();
        }

        #region Properties

        /// <summary>
        /// Gets or Set a value indicating whether an credentials are valid or not.
        /// Returns <c>true</c> if the credentials are valid; otherwise, <c>false</c>.
        /// </summary>
        internal bool IsCredentialValid { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async void ValidateCredential()
        {
            #region Azure OpenAI
            // Use below method for Azure Open AI
            this.GetAzureOpenAIKernal();
            #endregion


            if (isAlreadyValidated)
            {
                return;
            }

            try
            {
                if (client != null)
                {
                    await client!.CompleteAsync("Hello, Test Check");
                    chatHistory = string.Empty;
                    IsCredentialValid = true;
                    isAlreadyValidated = true;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// To get the Azure OpenAI method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            try
            {
                var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key)).AsChatClient(modelId: deploymentName);
                this.client = client;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets the AI response.
        /// </summary>
        /// <param name="userPrompt">The prompt.</param>
        /// <returns>The AI response.</returns>
        internal async Task<string> GetAIResponse(string userPrompt)
        {
            if (IsCredentialValid && client != null)
            {
                chatHistory = string.Empty;
                // Add the system message and user message to the options
                chatHistory = chatHistory + "You are a predictive analytics assistant.";
                chatHistory = chatHistory + userPrompt;
                try
                {
                    var response = await client.CompleteAsync(chatHistory);
                    return response.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        #endregion
    }
}