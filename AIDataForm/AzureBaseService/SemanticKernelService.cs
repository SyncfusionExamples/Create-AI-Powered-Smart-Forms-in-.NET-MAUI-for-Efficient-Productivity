namespace AIDataForm
{
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    internal  class SemanticKernelService
    {
        #region Fields

        /// <summary>
        /// The EndPoint
        /// </summary>
        private const string endpoint = "https://YOUR_ACCOUNT.openai.azure.com/";

        /// <summary>
        /// The Deployment name
        /// </summary>
        private const string deploymentName = "deployment name";

        /// <summary>
        /// The Image Deployment name
        /// </summary>
        private const string imageDeploymentName = "IMAGE_MODEL_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string key = "API key";

        /// <summary>
        /// The chat completion service
        /// </summary>
        private IChatCompletionService? chatCompletions;

        /// <summary>
        /// The kernel
        /// </summary>
        private Kernel? kernel;

        /// <summary>
        /// The chat history
        /// </summary>
        private ChatHistory? chatHistory;

        /// <summary>
        /// The credential valid field
        /// </summary>
        private static bool isCredentialValid = false;

        /// <summary>
        /// The already credential validated field
        /// </summary>
        private static bool isAlreadyValidated = false;

        /// <summary>
        /// The Uri result field
        /// </summary>
        private Uri? uriResult;

        #endregion

        public SemanticKernelService()
        {
            ValidateCredential();
        }

        #region Properties

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
        /// Gets or sets a value indicating the chat history object
        /// </summary>
        public ChatHistory? ChatHistory
        {
            get
            {
                return chatHistory;
            }
            set
            {
                chatHistory = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the chat completions object
        /// </summary>
        public IChatCompletionService? ChatCompletions
        {
            get
            {
                return chatCompletions;
            }
            set
            {
                chatCompletions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the kernel object
        /// </summary>
        public Kernel? Kernel
        {
            get
            {
                return kernel;
            }
            set
            {
                kernel = value;
            }
        }

        #endregion

        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async void ValidateCredential()
        {
            #region Azure OpenAI
            // Use below method for Azure Open AI
            this.GetAzureOpenAIKernal();
            #endregion

            #region Google Gemini
            // Use below method for Google Gemini
            //this.GetGoogleGiminiAIKernal();
            #endregion

            if (isAlreadyValidated)
            {
                return;
            }
            bool isValidUri = Uri.TryCreate(endpoint, UriKind.Absolute, out uriResult)
                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValidUri || !endpoint.Contains("http") || string.IsNullOrEmpty(key) || key.Contains("API key") || string.IsNullOrEmpty(deploymentName) || deploymentName.Contains("deployment name") || string.IsNullOrEmpty(imageDeploymentName))
            {
                return;
            }
            try
            {
                if (ChatHistory != null && chatCompletions != null)
                {
                    // test the semantic kernel with message.
                    ChatHistory.AddSystemMessage("Hello, Test Check");
                    await chatCompletions.GetChatMessageContentAsync(chatHistory: ChatHistory, kernel: kernel);
                }
            }
            catch (Exception)
            {
                // Handle any exceptions that indicate the credentials or endpoint are invalid.               
                return;
            }
            IsCredentialValid = true;
            isAlreadyValidated = true;
        }

        #region Azure OpenAI
        /// <summary>
        /// To get the Azure open AI kernel method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            // Create the chat history
            chatHistory = new ChatHistory();
            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, key);

            // Get the kernel from build
            kernel = builder.Build();

            //Get the chat completions from kernel
            chatCompletions = kernel.GetRequiredService<IChatCompletionService>();
        }
        #endregion

        #region Google Gemini
        /// <summary>
        /// To get the Google Gemini ai Kernel
        /// </summary>
        private void GetGoogleGiminiAIKernal()
        {
            //            //First Add the below package to the application
            //            add package Microsoft.SemanticKernel.Connectors.Google

            //            // Create the chat history
            //            chatHistory = new ChatHistory();
            //            #pragma warning disable SKEXP0070
            //            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            //            kernelBuilder.AddGoogleAIGeminiChatCompletion(modelId: "NAME_OF_MODEL", apiKey: key);
            //            Kernel kernel = kernelBuilder.Build();

            //            //Get the chat completions from Kernel
            //            chatCompletions = kernel.GetRequiredService<IChatCompletionService>();
        }

        #endregion

        /// <summary>
        /// Retrieves an answer from the deployment name model using the provided user prompt.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        /// <returns>The AI response.</returns>
        internal async Task<string> GetAnswerFromGPT(string userPrompt)
        {
            if (IsCredentialValid && ChatCompletions != null && ChatHistory != null)
            {
                ChatHistory.Clear();
                // Add the user's prompt as a user message to the conversation.
                ChatHistory.AddSystemMessage("You are a predictive analytics assistant.");
                // Add the user's prompt as a user message to the conversation.
                ChatHistory.AddUserMessage(userPrompt);
                try
                {
                    //// Send the chat completion request to the OpenAI API and await the response.
                    var response = await ChatCompletions.GetChatMessageContentAsync(chatHistory: ChatHistory, kernel: Kernel);
                    return response.ToString();
                }
                catch
                {
                    // If an exception occurs (e.g., network issues, API errors), return an empty string.
                    return "";
                }
            }

            return "";
        }
    }
}