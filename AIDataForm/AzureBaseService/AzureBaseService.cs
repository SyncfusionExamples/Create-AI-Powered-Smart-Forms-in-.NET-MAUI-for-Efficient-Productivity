using Azure;
using Azure.AI.OpenAI;

namespace AIDataForm
{
    public class AzureAIServices : AzureBaseService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAIServices"/> class.
        /// </summary>
        public AzureAIServices()
        {
            if (IsCredentialValid)
            {
                this.chatCompletions = new ChatCompletionsOptions
                {
                    DeploymentName = deploymentName,
                    Temperature = (float)0.5,
                    MaxTokens = 800,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                };
            }
        }

        /// <summary>
        /// Retrieves an answer from the deployment name model using the provided user prompt.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        /// <returns>The AI response.</returns>
        public async Task<string> GetResultsFromAI(string userPrompt)
        {
            if (this.Client != null && this.chatCompletions != null)
            {
                // Add the system message and user message to the options
                this.chatCompletions.Messages.Add(new ChatRequestSystemMessage("You are a predictive analytics assistant."));
                this.chatCompletions.Messages.Add(new ChatRequestUserMessage(userPrompt));
                try
                {
                    var response = await Client.GetChatCompletionsAsync(this.chatCompletions);
                    return response.Value.Choices[0].Message.Content;
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }

    public abstract class AzureBaseService
    {

        /// <summary>
        /// The EndPoint
        /// </summary>
        private const string endpoint = "https://YOUR_ACCOUNT.openai.azure.com/";

        /// <summary>
        /// The Deployment name
        /// </summary>
        internal const string deploymentName = "deployment name";

        /// <summary>
        /// The Image Deployment name
        /// </summary>
        internal const string imageDeploymentName = "IMAGE_MODEL_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string key = "API key";

        /// <summary>
        /// The AzureOpenAI client
        /// </summary>
        private OpenAIClient? client;

        /// <summary>
        /// The ChatCompletion option
        /// </summary>
        internal ChatCompletionsOptions? chatCompletions;

        private static bool isCredentialValid = false;

        private static bool isAlreadyValidated = false;

        private Uri? uriResult;

        public AzureBaseService()
        {
            ValidateCredential();
        }

        /// <summary>
        /// Get or Set Azure OpenAI client.
        /// </summary>
        public OpenAIClient? Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
            }
        }

        /// <summary>
        /// Gets or Set a value indicating whether an credentials are valid or not.
        /// Returns <c>true</c> if the credentials are valid; otherwise, <c>false</c>.
        /// </summary>
        public static bool IsCredentialValid
        {
            get
            {
                return isCredentialValid;
            }
            set
            {
                isCredentialValid = value;
            }
        }


        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private void ValidateCredential()
        {
            var options = new OpenAIClientOptions
            {
                Retry =
                {
                    MaxRetries = 1,  // Set the maximum number of retries to 1.
                }
            };
            if (isCredentialValid && Client == null)
            {
                Client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key), options);
            }
            if(isAlreadyValidated)
            {
                return;
            }
            bool isValidUri = Uri.TryCreate(endpoint, UriKind.Absolute, out uriResult)
                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValidUri || !endpoint.Contains("http") || string.IsNullOrEmpty(key) || key.Contains("API key") || string.IsNullOrEmpty(deploymentName) || deploymentName.Contains("deployment name") || string.IsNullOrEmpty(imageDeploymentName)) 
            {
                ShowAlertAsync();
                return;
            }
            try
            {
                // Initialize the OpenAI client for creadential check.
                if (Client == null)
                {
                    Client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key), options);
                }
                var chatCompletionsTest = new ChatCompletionsOptions
                {
                    DeploymentName = deploymentName,
                    MaxTokens = 50,
                };        
                chatCompletionsTest.Messages.Add(new ChatRequestSystemMessage("Hello, Test Check"));
                var result = Task.Run(async () => await Client.GetChatCompletionsAsync(chatCompletionsTest)).Result;
                if (result.GetRawResponse().Status != 200)
                {
                    ShowAlertAsync();
                    return;
                }
            }
            catch (Exception)
            {
                // Handle any exceptions that indicate the credentials or endpoint are invalid.               
                ShowAlertAsync();
                return;
            }
            IsCredentialValid = true;
            isAlreadyValidated = true;
        }

        /// <summary>
        /// Show Alert Popup
        /// </summary>
        private async void ShowAlertAsync()
        {
            if (Application.Current?.MainPage != null && !IsCredentialValid)
            {
                isAlreadyValidated = true;
                await Application.Current.MainPage.DisplayAlert("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
            }
        }

    }

}
