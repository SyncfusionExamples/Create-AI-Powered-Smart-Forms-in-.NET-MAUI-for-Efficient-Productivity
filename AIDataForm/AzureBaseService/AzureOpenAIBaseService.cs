namespace AIDataForm
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.AI.OpenAI;
    using Microsoft.Extensions.AI;

    /// <summary>
    /// The Azure OpenAI Base Service.
    /// </summary>
    internal class AzureOpenAIBaseService
    {
        /// <summary>
        /// The Azure OpenAI EndPoint
        /// </summary>
        private const string Endpoint = "AZURE_OPENAI_ENDPOINT";

        /// <summary>
        /// The Deployment name
        /// </summary>
        private const string DeploymentName = "DEPLOYMENT_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string Key = "API_KEY";

        /// <summary>
        /// The OpenAI.
        /// </summary>
        private IChatClient? client;

        /// <summary>
        /// Indicates whether the credentials have already been validated.
        /// </summary>
        private bool isAlreadyValidated;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAIBaseService"/> class.
        /// </summary>
        internal AzureOpenAIBaseService()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the credential endpoint valid.
        /// </summary>
        internal bool IsCredentialValid { get; private set; }

        /// <summary>
        /// Initializes the Azure OpenAI service and validates the credentials.
        /// </summary>
        /// <returns>A task that completes when initialization is done.</returns>
        internal async Task InitializeAsync()
        {
            if (this.isAlreadyValidated)
            {
                return;
            }

            try
            {
                var azureClient = new AzureOpenAIClient(new Uri(Endpoint), new AzureKeyCredential(Key));
                this.client = azureClient.AsChatClient(modelId: DeploymentName);

                var completion = await this.client.CompleteAsync("Hello");
                string text = ExtractText(completion);

                this.IsCredentialValid = !string.IsNullOrWhiteSpace(text);
                this.isAlreadyValidated = this.IsCredentialValid;
            }
            catch
            {
                this.IsCredentialValid = false;
                this.isAlreadyValidated = false;
            }
        }

        /// <summary>
        /// Gets the AI response for the given user prompt.
        /// </summary>
        /// <param name="userPrompt">The string value of the prompt.</param>
        /// <returns>A task that returns the AI response as a string.///.</returns>
        internal async Task<string> GetAIResponse(string userPrompt)
        {
            if (!this.IsCredentialValid || this.client == null)
            {
                return string.Empty;
            }

            try
            {
                var completion = await this.client.CompleteAsync(userPrompt);
                return Normalize(ExtractText(completion));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text from the ChatCompletion object.
        /// </summary>
        /// <param name="completion">The ChatCompletion result returned by the AI service.</param>
        /// <returns>
        /// The extracted text response, or an empty string if no content is available.
        /// </returns>
        private static string ExtractText(ChatCompletion? completion)
        {
            if (completion == null)
            {
                return string.Empty;
            }

            var msg = completion.Message;
            if (msg != null)
            {
                if (!string.IsNullOrWhiteSpace(msg.Text))
                {
                    return msg.Text;
                }

                if (msg.Contents != null)
                {
                    var textParts = msg.Contents
                        .OfType<TextContent>()
                        .Select(c => c.Text)
                        .Where(t => !string.IsNullOrWhiteSpace(t));
                    var combined = string.Join(string.Empty, textParts);
                    if (!string.IsNullOrWhiteSpace(combined))
                    {
                        return combined;
                    }
                }
            }

            return completion.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Normalizes the given text.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>
        /// The normalized text, or an empty string if the input is null.
        /// </returns>
        private static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Trim().Trim('"').Trim();
        }
    }
}