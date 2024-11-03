namespace AIDataForm
{
    using Syncfusion.Maui.AIAssistView;
    using System;
    using Syncfusion.Maui.Core;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Syncfusion.Maui.DataForm;
    using Syncfusion.Maui.Buttons;
    using Syncfusion.Maui.Popup;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The data form assist view behavior.
    /// </summary>
    public class DataFormAssistViewBehavior : Behavior<SfAIAssistView>
    {
        /// <summary>
        /// Prompt request collection.
        /// </summary>
        private string[] OfflineFormSuggestions =
        [
          "Contact Form",
          "Feedback Form"
        ];

        /// <summary>
        /// The contact form suggestions.
        /// </summary>
        private string[] ContactFormActions =
        [
            "Add Email",
            "Remove Last Item",
            "Change Tittle as User Details",
        ];

        /// <summary>
        /// The contact form suggestions.
        /// </summary>
        private string[] FeedbackFormActions =
        [
            "Remove Product Version",
            "Change Tittle as Feedback Form",
        ];

        /// <summary>
        /// Common Suggestions.
        /// </summary>
        private string[] YesOrNoSuggestions =
        [
            "Yes",
            "No",
            "Main Menu"
        ];

        private string? offlineForm;

        /// <summary>
        /// Holds the assist view instance.
        /// </summary>
        private SfAIAssistView? assistView;

        /// <summary>
        /// Holds the azure AI services.
        /// </summary>
        private SemanticKernelService semanticKernelService = new SemanticKernelService();

        /// <summary>
        /// Gets or sets the data form generator model.
        /// </summary>
        public DataFormGeneratorModel? DataFormGeneratorModel { get; set; }

        /// <summary>
        /// Gets or sets the data form name label.
        /// </summary>
        public Label? DataFormNameLabel { get; set; }

        /// <summary>
        /// Holds the busy indicator instance.
        /// </summary>
        public SfBusyIndicator? BusyIndicator { get; set; }

        /// <summary>
        /// Gets or sets the feedback form.
        /// </summary>
        public SfDataForm? DataForm { get; set; }

        /// <summary>
        /// Gets or sets the entry.
        /// </summary>
        public Editor? Entry { get; set; }

        /// <summary>
        /// Gets or sets the create button.
        /// </summary>
        public Button? CreateButton { get; set; }

        /// <summary>
        /// Gets or sets the ai action button.
        /// </summary>
        public SfButton? AIActionButton { get; set; }

        /// <summary>
        /// Gets or sets the close button.
        /// </summary>
        public Button? CloseButton { get; set; }

        /// <summary>
        /// On attached method.
        /// </summary>
        /// <param name="bindable">The bindable element.</param>
        protected override void OnAttachedTo(SfAIAssistView bindable)
        {
            base.OnAttachedTo(bindable);
            this.assistView = bindable;
          
            if (this.assistView != null)
            {
                this.assistView.Request += this.OnAssistViewRequest;
            }

            if (this.CreateButton != null)
            {
                this.CreateButton.Clicked += this.OnCreateButtonClicked;
            }

            if(this.CloseButton != null)
            {
                this.CloseButton.Clicked += CloseButton_Clicked;
            }

            if (this.AIActionButton != null)
            {
                this.AIActionButton.Clicked += this.OnAIActionButtonClicked;
            }
        }

        private void CloseButton_Clicked(object? sender, EventArgs e)
        {
            this.DataFormGeneratorModel.ShowAssistView = false;
        }

        /// <summary>
        /// Method triggers on AI action button clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnAIActionButtonClicked(object? sender, EventArgs e)
        {
            this.DataFormGeneratorModel.ShowAssistView = true;
        }

        /// <summary>
        /// On Detached method.
        /// </summary>
        /// <param name="bindable">The bindable element.</param>
        protected override void OnDetachingFrom(SfAIAssistView bindable)
        {
            base.OnDetachingFrom(bindable);
            if (this.assistView != null)
            {
                this.assistView.Request -= this.OnAssistViewRequest;
            }

            if (this.CreateButton != null)
            {
                this.CreateButton.Clicked -= this.OnCreateButtonClicked;
            }

            if (this.AIActionButton != null)
            {
                this.AIActionButton.Clicked -= this.OnAIActionButtonClicked;
            }
        }

        /// <summary>
        /// Method triggers on create button clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private async void OnCreateButtonClicked(object? sender, EventArgs e)
        {
            UpdateBusyIndicator(true);

            if (SemanticKernelService.IsCredentialValid)
            {
                this.GetDataFormFromAI(this.Entry!.Text);
            }
            else
            {
                UpdateCreateVisibility();
                UpdateBusyIndicator(false);
             
                AssistItemSuggestion assistItemSuggestion = this.GetSubjectSuggestion();
                AssistItem assistItem = new AssistItem() { Text = "You are in offline mode. Please select one of the forms below.", Suggestion = assistItemSuggestion, ShowAssistItemFooter = false };
                this.DataFormGeneratorModel!.Messages.Add(assistItem);
            }
        }

        /// <summary>
        /// Method to provide AI request.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private async void OnAssistViewRequest(object? sender, RequestEventArgs e)
        {
            string requestText = e.RequestItem.Text;
            if (SemanticKernelService.IsCredentialValid)
            {
                this.DataFormGeneratorModel.ShowOfflineLabel = false;
                this.GetDataFormFromAI(requestText);
                return;
            }

            await CreateOfflineDataForm(requestText);
        }

        private async Task CreateOfflineDataForm(string requestText)
        {
            if (requestText == this.OfflineFormSuggestions[0])
            {
                offlineForm = "ConatctForm";
                this.InitializeOfflineContactDataForm();
                this.ChangeDataFormTitle(this.OfflineFormSuggestions[0]);
                await this.AddMessageWithDelayAsync("Please select your action below...", this.GetContactFormSuggestion());
            }
            else if (requestText == this.ContactFormActions[0])
            {
                this.DataForm!.Items.Add(new DataFormTextItem() { FieldName = "Email", Keyboard = Keyboard.Email });
                await this.AddMessageWithDelayAsync("The Email editor added successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.ContactFormActions[1])
            {
                this.DataForm!.Items.RemoveAt(this.DataForm!.Items.Count - 1);
                await this.AddMessageWithDelayAsync("The last item removed successfully.");
                await AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.ContactFormActions[2])
            {
                this.ChangeDataFormTitle("User Details");
                await this.AddMessageWithDelayAsync("The title has changed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.OfflineFormSuggestions[1])
            {
                offlineForm = "FeedbackForm";
                this.InitializeOfflineFeedbackDataForm();
                this.ChangeDataFormTitle("Product feedback");
                await this.AddMessageWithDelayAsync("Please select your action below...", this.GetFeedbackFormSuggestion());
            }
            else if (requestText == this.FeedbackFormActions[0])
            {
                if (this.DataForm != null && this.DataForm.Items != null && this.DataForm.Items.Count > 0)
                {
                    var removeItem = this.DataForm.Items.FirstOrDefault(x => (x as DataFormItem).FieldName == "ProductVersion");
                    if (removeItem != null)
                        this.DataForm.Items.Remove(removeItem);
                }
                await this.AddMessageWithDelayAsync("The Product Version editor has removed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.FeedbackFormActions[1])
            {
                this.ChangeDataFormTitle("Feedback Form");
                await this.AddMessageWithDelayAsync("The title has changed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.YesOrNoSuggestions[0])
            {
                if (offlineForm == "FeedbackForm")
                {
                    await this.AddMessageWithDelayAsync("Please select any of the action below...", this.GetFeedbackFormSuggestion());
                }
                else
                {
                    await this.AddMessageWithDelayAsync("Please select any of the action below...", this.GetContactFormSuggestion());
                }
            }
            else if (requestText == this.YesOrNoSuggestions[1])
            {
                await this.AddMessageWithDelayAsync("Thank you..!");
            }
            else if (requestText == this.YesOrNoSuggestions[2])
            {
                await this.AddMessageWithDelayAsync("You are offline. Please select any other form...");
                await this.AddMessageWithDelayAsync("Please select any of the forms below...", this.GetSubjectSuggestion());
            }
        }

        private void InitializeOfflineContactDataForm()
        {
            ObservableCollection<DataFormViewItem> dataFormViewItems = new ObservableCollection<DataFormViewItem>
            {
                new DataFormTextItem() { FieldName = "FirstName", LabelText = "First Name" },
                new DataFormTextItem() { FieldName = "LastName", LabelText="Last Name" },
                new DataFormTextItem() { FieldName = "Mobile",  },
                new DataFormTextItem() { FieldName = "LandLine",  },
                new DataFormTextItem() { FieldName = "Address" },
                new DataFormTextItem() { FieldName = "City" },
                new DataFormTextItem() { FieldName = "State" },
                new DataFormTextItem() { FieldName = "ZipCode" }
            };
            this.DataForm!.Items = dataFormViewItems;
            this.DataFormGeneratorModel.ShowSubmitButton = true;
            this.DataFormGeneratorModel.ShowOfflineLabel = false;

        }

        private void InitializeOfflineFeedbackDataForm()
        {
            ObservableCollection<DataFormViewItem> dataFormViewItems = new ObservableCollection<DataFormViewItem>
            {
                new DataFormTextItem() { FieldName = "Name"},
                new DataFormTextItem() { FieldName = "Email", Keyboard = Keyboard.Email},
                new DataFormTextItem() { FieldName = "ProductName", LabelText = "Product Name" },
                new DataFormTextItem() { FieldName = "ProductVersion", LabelText = "Product Version"  },
                new DataFormNumericItem() { FieldName = "Rating" },
                new DataFormMultilineItem() { FieldName = "Comments" },
            };
            this.DataForm!.Items = dataFormViewItems;
            this.DataFormGeneratorModel.ShowSubmitButton = true;
            this.DataFormGeneratorModel.ShowOfflineLabel = false;
        }

        private void UpdateBusyIndicator(bool value)
        {
            if (this.BusyIndicator != null)
            {
                this.BusyIndicator.IsVisible = value;
                this.BusyIndicator.IsRunning = value;
            }
        }

        /// <summary>
        /// Method to update the title of the data form.
        /// </summary>
        /// <param name="newTitle">The new title.</param>
        private void ChangeDataFormTitle(string newTitle)
        {
            this.DataFormNameLabel!.Text = newTitle;
        }

        /// <summary>
        /// Method to add the response message in the chat bot page.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="suggestions">The assist item suggestions.</param>
        /// <returns>Returns task.</returns>
        private async Task AddMessageWithDelayAsync(string text, AssistItemSuggestion? suggestions = null)
        {
            AssistItem assistItem = new AssistItem() { Text = text, Suggestion = suggestions!, ShowAssistItemFooter = false };
            await Task.Delay(1000).ConfigureAwait(true);
            this.DataFormGeneratorModel?.Messages.Add(assistItem);
        }

        /// <summary>
        /// Method to get the subject suggestions.
        /// </summary>
        /// <returns>Returns AssistItemSuggestion.</returns>
        private AssistItemSuggestion GetSubjectSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var subjectsuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = OfflineFormSuggestions[0] },
                new AssistSuggestion() { Text = OfflineFormSuggestions[1] }
            };
            chatSubjectSuggestions.Items = subjectsuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Method to get the subject suggestions.
        /// </summary>
        /// <returns>Returns AssistItemSuggestion.</returns>
        private AssistItemSuggestion GetContactFormSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = ContactFormActions[0] },
                new AssistSuggestion() { Text = ContactFormActions[1] },
                new AssistSuggestion() { Text = ContactFormActions[2] }
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        private AssistItemSuggestion GetFeedbackFormSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = FeedbackFormActions[0] },
                new AssistSuggestion() { Text = FeedbackFormActions[1] },
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Method to get the yes or no suggestions.
        /// </summary>
        /// <returns>Returns AssistItemSuggestion.</returns>
        private AssistItemSuggestion GetYesOrNoSuggestions()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = YesOrNoSuggestions[0] },
                new AssistSuggestion() { Text = YesOrNoSuggestions[1] },
                new AssistSuggestion() { Text = YesOrNoSuggestions[2] }
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Method to get the data form from AI.
        /// </summary>
        /// <param name="userPrompt">Th user prompt.</param>
        internal async void GetDataFormFromAI(string userPrompt)
        {
            string prompt = $"Given the user's input: {userPrompt}, determine the most appropriate single action to take. " +
                $"The options are 'Add', 'Remove', 'Replace', 'Insert', 'New Form', 'Change Title', or 'No Change'" +
                " Without additional formatting and special characters like backticks, newlines, or extra spaces.";

            var response = await this.semanticKernelService.GetAnswerFromGPT(prompt);

            if (string.IsNullOrEmpty(response))
            {
                AssistItem subjectMessage = new AssistItem() { Text = "Please try again...", ShowAssistItemFooter = false };
                this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
                UpdateCreateVisibility();
                UpdateBusyIndicator(false);
            }
            else
            {
                if (response == "Add")
                {
                    this.EditDataForm(userPrompt, "Add");
                }
                else if (response == "Remove")
                {
                    this.EditDataForm(userPrompt, "Remove");
                }
                else if (response == "Replace")
                {
                    this.EditDataForm(userPrompt, "Replace");
                }
                else if (response == "New Form")
                {
                    this.DataFormGeneratorModel.ShowOfflineLabel = false;
                    this.GenerateAIDataForm(userPrompt);
                }
                else if (response == "Change Title")
                {
                    string dataFormNamePrompt = $"Change the title for data form based on user prompt: {userPrompt}. Provide only the title, with no additional explanation";
                    string getDataFormName = await this.semanticKernelService.GetAnswerFromGPT(dataFormNamePrompt);
                    this.DataFormNameLabel!.Text = getDataFormName;
                    AssistItem subjectMessage = new AssistItem() { Text = "The Data Form title changed successfully...", ShowAssistItemFooter = false };
                    this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
                }
                else
                {
                    UpdateBusyIndicator(false);
                    await App.Current.MainPage.DisplayAlert("", "Please enter valid inputs.", "OK");
                }
            }
        }

        /// <summary>
        /// Method to generate the data form using AI.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        private async void GenerateAIDataForm(string userPrompt)
        {
            string dataFormNamePrompt = $"Generate a title for a data form based on the following string: {userPrompt}. The title should clearly reflect the purpose of the data form in general term. Provide only the title, with no additional explanation";
            string getDataFormName = await this.semanticKernelService.GetAnswerFromGPT(dataFormNamePrompt);
            this.DataFormNameLabel!.Text = getDataFormName;

            string prompt = $"Generate a data form based on the user prompt: {userPrompt}.";
            string condition = "Property names must be in PascalCase. " +
                "Must be property names and its value " +
                "Without additional formatting characters like backticks, newlines, or extra spaces. " +
                "and map each property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
                "The result must be in JSON format" +
                "Without additional formatting characters like backticks, newlines, or extra spaces.";

            var typeResponse = await this.semanticKernelService.GetAnswerFromGPT(prompt + condition);

            var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);

            if (this.DataForm != null)
            {
                var items = new ObservableCollection<DataFormViewItem>();
                foreach (var data in dataFormTypes!)
                {
                    DataFormItem dataFormItem = GenerateDataFormItems(data.Value?.ToString(), data.Key);
                    if (dataFormItem != null)
                        items.Add(dataFormItem);
                }

                this.DataForm.Items = items;
            }

            AssistItem subjectMessage = new AssistItem() { Text = "As per your comment data form created successfully...", ShowAssistItemFooter = false };
            this.DataFormGeneratorModel?.Messages.Add(subjectMessage);

            UpdateCreateVisibility();
            UpdateBusyIndicator(false);
        }

        private void UpdateCreateVisibility()
        {
            this.DataFormGeneratorModel.ShowInputView = false;
            this.DataFormGeneratorModel.ShowDataForm = true;

         
        }

        /// <summary>
        /// Method to edit the data form.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        private async void EditDataForm(string userPrompt, string action)
        {
            if (this.DataForm!.Items != null)
            {
                if (action == "Add")
                {
                    userPrompt = userPrompt.Replace("add", "", StringComparison.OrdinalIgnoreCase).Trim();
                    string prompt = $"Generate a Property name based on the user prompt: {userPrompt}.";
                    string condition = "The result must be in string" +
                        "Property name must be in PascalCase, without asking questions, or including extra explanations. " +
                        "Without additional formatting characters like backticks, newlines, or extra spaces." +
                        $" and map that property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
       "The result must be in JSON format" +
          "Without additional formatting characters like backticks, newlines, or extra spaces.";
                    var typeResponse = await this.semanticKernelService.GetAnswerFromGPT(prompt + condition);

                    var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);
                    if (dataFormTypes != null)
                    {
                        if (dataFormTypes != null && dataFormTypes.Count > 0)
                        {
                            var newItem = dataFormTypes.FirstOrDefault();
                            if (newItem.Value != null)
                            {
                                DataFormItem dataFormItem = GenerateDataFormItems(newItem.Value.ToString(), newItem.Key);
                                if (dataFormItem != null)
                                {
                                    this.DataForm.Items.Add(dataFormItem);
                                    UpdateChangesResponse();
                                }

                            }
                        }
                    }
                }
                else if (action == "Remove")
                {
                    userPrompt = userPrompt.Replace("remove", "", StringComparison.OrdinalIgnoreCase).Trim();
                    string prompt = $"Generate a Property name based on the user prompt: {userPrompt}.";
                    string condition = "The result must be in string" +
                        "Property name must be in PascalCase, without asking questions, or including extra explanations. " +
                        "Without additional formatting characters like backticks, newlines, or extra spaces.";
                    string response = await this.semanticKernelService.GetAnswerFromGPT(prompt + condition);

                    var removeItem = this.DataForm!.Items.FirstOrDefault(x => x != null && (x as DataFormItem).FieldName == response);
                    if (removeItem != null)
                    {
                        this.DataForm.Items.Remove(removeItem);
                        UpdateChangesResponse();
                    }
                }
                else if (action == "Replace")
                {
                    string pattern = @"Replace\s+(.*?)\s+(with|by|to)\s+(.*)";
                    Match match = Regex.Match(userPrompt, pattern, RegexOptions.IgnoreCase);

                    if (match.Success && match.Groups.Count == 4)
                    {
                        string prompt = $"Based on the user input '{match.Groups[1].Value.Trim()}', generate a Property name.";
                        string condition = "The result must be a valid Property name in PascalCase format. " +
                                           "Do not include explanations, questions, or additional characters like backticks, newlines, or spaces. " +
                                           "Return only the Property name in PascalCase.";

                        string response = await this.semanticKernelService.GetAnswerFromGPT(prompt + condition);

                        string prompt1 = $"Generate a Property name from {match.Groups[3].Value.Trim()}.";
                        string condition1 = "The result must be in string and Property name must be in PascalCase,  without asking questions, or including extra explanations. " +
                           "Without additional formatting characters like backticks, newlines, or extra spaces." +
                           " map that generated property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
                           "The result must be in JSON format" +
                           "without extra formatting like backticks, newlines, or special characters.";
                        var typeResponse = await this.semanticKernelService.GetAnswerFromGPT(prompt1 + condition1);

                        var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);

                        if (dataFormTypes != null && !string.IsNullOrEmpty(response))
                        {
                            if (dataFormTypes != null && dataFormTypes.Count > 0)
                            {
                                var newItem = dataFormTypes.FirstOrDefault();
                                var removeItem = this.DataForm.Items.FirstOrDefault(x => (x as DataFormItem).FieldName == response);
                                if (removeItem != null && newItem.Value != null)
                                {
                                    int index = DataForm.Items.IndexOf(removeItem);
                                    this.DataForm.Items[index] = GenerateDataFormItems(newItem.Value.ToString(), newItem.Key);
                                    UpdateChangesResponse();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateChangesResponse()
        {
            AssistItem subjectMessage = new AssistItem() { Text = "Your modifications have been completed successfully....", ShowAssistItemFooter = false };
            this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
        }

        private DataFormItem? GenerateDataFormItems(string dataFormItemType, string fieldName)
        {
            DataFormItem dataFormItem = dataFormItemType switch
            {
                "DataFormTextItem" => new DataFormTextItem(),
                "DataFormMultiLineTextItem" => new DataFormMultilineItem(),
                "DataFormPasswordItem" => new DataFormPasswordItem(),
                "DataFormNumericItem" => new DataFormNumericItem(),
                "DataFormMaskedTextItem" => new DataFormMaskedTextItem(),
                "DataFormDateItem" => new DataFormDateItem(),
                "DataFormTimeItem" => new DataFormTimeItem(),
                "DataFormCheckBoxItem" => new DataFormCheckBoxItem(),
                "DataFormSwitchItem" => new DataFormSwitchItem(),
                "DataFormPickerItem" => new DataFormPickerItem(),
                "DataFormComboBoxItem" => new DataFormComboBoxItem(),
                "DataFormAutoCompleteItem" => new DataFormAutoCompleteItem(),
                "DataFormRadioGroupItem" => new DataFormRadioGroupItem(),
                "DataFormSegmentItem" => new DataFormSegmentItem(),
                _ => null
            };

            if (dataFormItem == null)
            {
                return null;
            }

            dataFormItem.FieldName = fieldName;
            dataFormItem.LabelText = GetLabelText(fieldName);

            if (fieldName == "Email" && dataFormItem is DataFormTextItem emailEditor)
            {
                emailEditor.Keyboard = Keyboard.Email;
            }

            return dataFormItem;
        }

        private string GetLabelText(string fieldName)
        {
            return Regex.Replace(fieldName, "(\\B[A-Z])", " $1");
        }
    }
}
