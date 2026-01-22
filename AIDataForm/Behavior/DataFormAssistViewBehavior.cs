namespace AIDataForm
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;
    using Newtonsoft.Json;
    using Syncfusion.Maui.AIAssistView;
    using Syncfusion.Maui.Buttons;
    using Syncfusion.Maui.Core;
    using Syncfusion.Maui.DataForm;

    /// <summary>
    /// Provides behavior that lets SfAIAssistView create and edit data forms from user prompts.
    /// </summary>
    public class DataFormAssistViewBehavior : Behavior<SfAIAssistView>
    {
        private readonly AzureOpenAIBaseService azureOpenAIBaseService = new AzureOpenAIBaseService();

        private string[] offlineFormSuggestions =
        [
            "Contact Form",
            "Feedback Form",
            "Employment Details"
        ];

        private string[] contactFormActions =
        [
            "Add Email",
            "Remove Last Item",
            "Change Title as User Details",
        ];

        private string[] employeeDetailsActions =
        [
            "Employee ID",
            "Remove Last detail",
            "Change Title as Employee registration",
        ];

        private string[] feedbackFormActions =
        [
            "Remove Product Version",
            "Change Title as Feedback Form",
        ];

        private string[] yesOrNoSuggestions =
        [
            "Yes",
            "No",
            "Main Menu"
        ];

        private string? offlineForm;
        private SfAIAssistView? assistView;
        private Animation? animation;

        /// <summary>
        /// Gets or sets the model used to generate the data form.
        /// </summary>
        public DataFormGeneratorModel? DataFormGeneratorModel { get; set; }

        /// <summary>
        /// Gets or sets the label control that displays the name of the data form.
        /// </summary>
        public Label? DataFormNameLabel { get; set; }

        /// <summary>
        /// Gets or sets the busy indicator control used to display loading or processing states.
        /// </summary>
        public SfBusyIndicator? BusyIndicator { get; set; }

        /// <summary>
        /// Gets or sets the data form control that is generated and modified based on user prompts.
        /// </summary>
        public SfDataForm? DataForm { get; set; }

        /// <summary>
        /// Gets or sets the entry control where users input their prompts for data form generation or modification.
        /// </summary>
        public Editor? Entry { get; set; }

        /// <summary>
        /// Gets or sets the button control that triggers a refresh action.
        /// </summary>
        public Button? RefreshButton { get; set; }

        /// <summary>
        /// Gets or sets the button instance used to initiate creation actions.
        /// </summary>
        public Button? CreateButton { get; set; }

        /// <summary>
        /// Gets or sets the button used to trigger AI-related actions in the user interface.
        /// </summary>
        public SfButton? AIActionButton { get; set; }

        /// <summary>
        /// Gets or sets the button that closes the window or dialog.
        /// </summary>
        public Button? CloseButton { get; set; }

        /// <summary>
        /// Processes the specified user input to create or modify an offline data form, updating the form and related messages based on the provided request.
        /// </summary>
        /// <param name="requestText">The user input text that determines the action to perform on the offline data form.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task CreateOfflineDataForm(string requestText)
        {
            if (requestText == this.offlineFormSuggestions[2])
            {
                this.offlineForm = "Employee Details";
                this.InitializeOfflineEmployeeDetails();
                this.ChangeDataFormTitle(this.offlineFormSuggestions[2]);
                await this.AddMessageWithDelayAsync("You are in offline mode. Please select your action below...", this.GetEmployeeDetailsSuggestion());
            }
            else if (requestText == this.employeeDetailsActions[0])
            {
                this.DataForm!.Items.Add(new DataFormTextItem() { FieldName = "Employee Id", Keyboard = Keyboard.Text });
                await this.AddMessageWithDelayAsync("The Employee id editor added successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.employeeDetailsActions[1])
            {
                this.DataForm!.Items.RemoveAt(this.DataForm!.Items.Count - 1);
                await this.AddMessageWithDelayAsync("The last item removed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.employeeDetailsActions[2])
            {
                this.ChangeDataFormTitle("Employee Registration");
                await this.AddMessageWithDelayAsync("The title has changed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }

            if (requestText == this.offlineFormSuggestions[0])
            {
                this.offlineForm = "ContactForm";
                this.InitializeOfflineContactDataForm();
                this.ChangeDataFormTitle(this.offlineFormSuggestions[0]);
                await this.AddMessageWithDelayAsync("You are in offline mode. Please select your action below...", this.GetContactFormSuggestion());
            }
            else if (requestText == this.contactFormActions[0])
            {
                this.DataForm!.Items.Add(new DataFormTextItem() { FieldName = "Email", Keyboard = Keyboard.Email });
                await this.AddMessageWithDelayAsync("The Email editor added successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.contactFormActions[1])
            {
                this.DataForm!.Items.RemoveAt(this.DataForm!.Items.Count - 1);
                await this.AddMessageWithDelayAsync("The last item removed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.contactFormActions[2])
            {
                this.ChangeDataFormTitle("User Details");
                await this.AddMessageWithDelayAsync("The title has changed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.offlineFormSuggestions[1])
            {
                this.offlineForm = "FeedbackForm";
                this.InitializeOfflineFeedbackDataForm();
                this.ChangeDataFormTitle("Product feedback");
                await this.AddMessageWithDelayAsync("You are in offline mode. Please select your action below...", this.GetFeedbackFormSuggestion());
            }
            else if (requestText == this.feedbackFormActions[0])
            {
                if (this.DataForm != null && this.DataForm.Items != null && this.DataForm.Items.Count > 0)
                {
                    var removeItem = this.DataForm.Items.FirstOrDefault(x => x != null && (x is DataFormItem dataFormItem) && dataFormItem.FieldName == "ProductVersion");
                    if (removeItem != null)
                    {
                        this.DataForm.Items.Remove(removeItem);
                    }
                }

                await this.AddMessageWithDelayAsync("The Product Version editor has removed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.feedbackFormActions[1])
            {
                this.ChangeDataFormTitle("Feedback Form");
                await this.AddMessageWithDelayAsync("The title has changed successfully.");
                await this.AddMessageWithDelayAsync("Do you want to edit..?", this.GetYesOrNoSuggestions());
            }
            else if (requestText == this.yesOrNoSuggestions[0])
            {
                if (this.offlineForm == "FeedbackForm")
                {
                    await this.AddMessageWithDelayAsync("Please select any of the action below...", this.GetFeedbackFormSuggestion());
                }
                else if (this.offlineForm == "ContactForm")
                {
                    await this.AddMessageWithDelayAsync("Please select any of the action below...", this.GetContactFormSuggestion());
                }
                else
                {
                    await this.AddMessageWithDelayAsync("Please select any of the action below...", this.GetEmployeeDetailsSuggestion());
                }
            }
            else if (requestText == this.yesOrNoSuggestions[1])
            {
                await this.AddMessageWithDelayAsync("Thank you..!");
            }
            else if (requestText == this.yesOrNoSuggestions[2])
            {
                if (this.DataFormGeneratorModel != null)
                {
                    this.DataFormGeneratorModel.Messages.Clear();
                    this.DataFormGeneratorModel.ShowDataForm = false;
                    this.DataFormGeneratorModel.ShowInputView = true;
                }
            }

            this.UpdateBusyIndicator(false);
        }

        /// <summary>
        /// Processes the user's prompt to determine the appropriate action for generating or modifying a data form using AI.
        /// </summary>
        /// <param name="userPrompt">The string value.</param>
        internal async void GetDataFormFromAI(string userPrompt)
        {
            string prompt = $"Given the user's input: {userPrompt}, determine the most appropriate single action to take. " +
            $"The options are 'Add', 'Add Values','PlaceholderText' ,'Remove', 'Replace', 'Insert', 'New Form', 'Change Title', or 'No Change'" +
            " Without additional formatting and special characters like backticks, newlines, or extra spaces.";

            var response = await this.azureOpenAIBaseService.GetAIResponse(prompt);
            if (string.IsNullOrEmpty(response))
            {
                AssistItem subjectMessage = new AssistItem() { Text = "Please try again...", ShowAssistItemFooter = false };
                this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
                this.UpdateCreateVisibility();
                this.UpdateBusyIndicator(false);
            }
            else
            {
                if (response == "New Form")
                {
                    if (this.DataFormGeneratorModel != null)
                    {
                        this.DataFormGeneratorModel.ShowOfflineLabel = false;
                    }

                    this.GenerateAIDataForm(userPrompt);
                }
                else if (response == "Change Title")
                {
                    string dataFormNamePrompt = $"Change the title for data form based on user prompt: {userPrompt}. Provide only the title, with no additional explanation";
                    string getDataFormName = await this.azureOpenAIBaseService.GetAIResponse(dataFormNamePrompt);
                    this.DataFormNameLabel!.Text = getDataFormName;
                    AssistItem subjectMessage = new AssistItem() { Text = "The Data Form title changed successfully...", ShowAssistItemFooter = false };
                    this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
                    this.UpdateBusyIndicator(false);
                }
                else
                {
                    this.EditDataForm(userPrompt, response);
                }
            }
        }

        /// <summary>
        /// Attaches the behavior to the specified SfAIAssistView and subscribes to relevant events.
        /// </summary>
        /// <param name="assistView">The SfAIAssistView instance to which the behavior is being attached. Cannot be null.</param>
        protected override void OnAttachedTo(SfAIAssistView assistView)
        {
            base.OnAttachedTo(assistView);
            this.assistView = assistView;
            this.animation = new Animation();

            // Initialize Azure client (fire-and-forget)
            _ = this.azureOpenAIBaseService.InitializeAsync();

            if (this.assistView != null)
            {
                this.assistView.Request += this.OnAssistViewRequest;
            }

            if (this.CreateButton != null)
            {
                this.CreateButton.Clicked += this.OnCreateButtonClicked;
            }

            if (this.CloseButton != null)
            {
                this.CloseButton.Clicked += this.CloseButton_Clicked;
            }

            if (this.RefreshButton != null)
            {
                this.RefreshButton.Clicked += this.RefreshButton_Clicked;
            }

            if (this.AIActionButton != null)
            {
                this.AIActionButton.Clicked += this.OnAIActionButtonClicked;
                this.StartAnimation();
            }

            if (this.Entry != null)
            {
                this.Entry.TextChanged += this.Entry_TextChanged;
            }
        }

        /// <summary>
        /// Called when the behavior is detached from the specified SfAIAssistView instance. Cleans up event handlers and resources associated with the view.
        /// </summary>
        /// <param name="assistView">The SfAIAssistView instance from which the behavior is being detached.</param>
        protected override void OnDetachingFrom(SfAIAssistView assistView)
        {
            base.OnDetachingFrom(assistView);

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

            if (this.CloseButton != null)
            {
                this.CloseButton.Clicked -= this.CloseButton_Clicked;
            }

            if (this.RefreshButton != null)
            {
                this.RefreshButton.Clicked -= this.RefreshButton_Clicked;
            }
        }

        /// <summary>
        /// Checks if the provided items source has any items.
        /// </summary>
        /// <param name="itemsSource">The object itemsource.</param>
        /// <returns>The bool value.</returns>
        private static bool HasAnyItems(object? itemsSource)
        {
            if (itemsSource is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return enumerator.MoveNext();
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            }

            return false;
        }

        /// <summary>
        /// Handles the TextChanged event for the entry control, enabling or disabling the create button based on whether the entry contains text.
        /// </summary>
        /// <param name="sender">The source of the event, typically the entry control whose text has changed.</param>
        /// <param name="e">The event data containing information about the text change.</param>
        private void Entry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (this.CreateButton != null && this.Entry != null)
            {
                this.CreateButton.IsEnabled = !string.IsNullOrEmpty(this.Entry.Text);
            }
        }

        /// <summary>
        /// Starts the bubble and fade animation effect on the AI action button if it is available.
        /// </summary>
        private async void StartAnimation()
        {
            if (this.AIActionButton != null && this.animation != null)
            {
                var bubbleEffect = new Animation(v => this.AIActionButton.Scale = v, 1, 1.15, Easing.CubicInOut);
                var fadeEffect = new Animation(v => this.AIActionButton.Opacity = v, 1, 0.5, Easing.CubicInOut);

                this.animation.Add(0, 0.5, bubbleEffect);
                this.animation.Add(0, 0.5, fadeEffect);
                this.animation.Add(0.5, 1, new Animation(v => this.AIActionButton.Scale = v, 1.15, 1, Easing.CubicInOut));
                this.animation.Add(0.5, 1, new Animation(v => this.AIActionButton.Opacity = v, 0.5, 1, Easing.CubicInOut));

                await Task.Delay(250);
                this.animation.Commit(this.AIActionButton, "BubbleEffect", length: 1500, easing: Easing.CubicInOut, repeat: () => true);
            }
        }

        /// <summary>
        /// Handles the Click event of the Refresh button, resetting the data form view and clearing any existing messages.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Refresh button.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void RefreshButton_Clicked(object? sender, EventArgs e)
        {
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.Messages.Clear();
                this.DataFormGeneratorModel.ShowInputView = true;
                this.DataFormGeneratorModel.ShowDataForm = false;
            }
        }

        /// <summary>
        /// Handles the Clicked event of the Close button by closing the assist view if it is currently open.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Close button.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void CloseButton_Clicked(object? sender, EventArgs e)
        {
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowAssistView = false;
            }
        }

        private void OnAIActionButtonClicked(object? sender, EventArgs e)
        {
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowAssistView = true;
            }
        }

        /// <summary>
        /// Handles the click event for the Create button, generating a data form using either Azure OpenAI or offline logic based on credential availability.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Create button.</param>
        /// <param name="e">An object that contains the event data.</param>
        private async void OnCreateButtonClicked(object? sender, EventArgs e)
        {
            this.UpdateBusyIndicator(true);
            if (this.Entry != null && this.DataFormGeneratorModel != null)
            {
                if (this.azureOpenAIBaseService.IsCredentialValid)
                {
                    this.GetDataFormFromAI(this.Entry.Text);
                }
                else
                {
                    if (this.DataFormGeneratorModel.FormTitle != null)
                    {
                        await this.CreateOfflineDataForm(this.DataFormGeneratorModel.FormTitle);
                    }
                    else
                    {
                        await this.CreateOfflineDataForm("Contact Form");
                    }

                    this.DataFormGeneratorModel.ShowInputView = false;
                    this.DataFormGeneratorModel.ShowDataForm = true;
                    this.DataFormGeneratorModel.ShowOfflineLabel = true;
                }
            }
        }

        /// <summary>
        /// Handles assist view requests by generating a data form using either the AI service or an offline method, depending on credential availability.
        /// </summary>
        /// <param name="sender">The source of the event. This parameter is typically the control that raised the event.</param>
        /// <param name="e">A RequestEventArgs object that contains the event data, including the request item text.</param>
        private async void OnAssistViewRequest(object? sender, RequestEventArgs e)
        {
            string requestText = e.RequestItem.Text;
            if (this.azureOpenAIBaseService.IsCredentialValid && this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowOfflineLabel = false;
                this.GetDataFormFromAI(requestText);
                return;
            }

            await this.CreateOfflineDataForm(requestText);
        }

        /// <summary>
        /// Initializes the data form with fields for offline contact information.
        /// </summary>
        private void InitializeOfflineContactDataForm()
        {
            var dataFormViewItems = new ObservableCollection<DataFormViewItem>
            {
                new DataFormTextItem() { FieldName = "FirstName", LabelText = "First Name" },
                new DataFormTextItem() { FieldName = "LastName", LabelText = "Last Name" },
                new DataFormTextItem() { FieldName = "Mobile" },
                new DataFormTextItem() { FieldName = "LandLine" },
                new DataFormTextItem() { FieldName = "Address" },
                new DataFormTextItem() { FieldName = "City" },
                new DataFormTextItem() { FieldName = "State" },
                new DataFormTextItem() { FieldName = "ZipCode" },
            };
            this.DataForm!.Items = dataFormViewItems;
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowSubmitButton = true;
                this.DataFormGeneratorModel.ShowOfflineLabel = false;
            }
        }

        /// <summary>
        /// Initializes the data form with fields for offline employee details and configures related UI elements.
        /// </summary>
        private void InitializeOfflineEmployeeDetails()
        {
            var dataFormViewItems = new ObservableCollection<DataFormViewItem>
            {
                new DataFormTextItem() { FieldName = "FirstName", LabelText = "First Name" },
                new DataFormTextItem() { FieldName = "LastName", LabelText = "Last Name" },
                new DataFormTextItem() { FieldName = "Designation" },
                new DataFormTextItem() { FieldName = "Experience" },
                new DataFormTextItem() { FieldName = "Mobile" },
            };
            this.DataForm!.Items = dataFormViewItems;
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowSubmitButton = true;
                this.DataFormGeneratorModel.ShowOfflineLabel = false;
            }
        }

        /// <summary>
        /// Initializes the data form with fields required for collecting offline feedback.
        /// </summary>
        private void InitializeOfflineFeedbackDataForm()
        {
            var dataFormViewItems = new ObservableCollection<DataFormViewItem>
            {
                new DataFormTextItem() { FieldName = "Name" },
                new DataFormTextItem() { FieldName = "Email", Keyboard = Keyboard.Email },
                new DataFormTextItem() { FieldName = "ProductName", LabelText = "Product Name" },
                new DataFormTextItem() { FieldName = "ProductVersion", LabelText = "Product Version" },
                new DataFormNumericItem() { FieldName = "Rating" },
                new DataFormMultilineItem() { FieldName = "Comments" },
            };
            this.DataForm!.Items = dataFormViewItems;
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowSubmitButton = true;
                this.DataFormGeneratorModel.ShowOfflineLabel = false;
            }
        }

        /// <summary>
        /// Updates the visibility and running state of the busy indicator based on the provided value.
        /// </summary>
        /// <param name="value">The bool value.</param>
        private void UpdateBusyIndicator(bool value)
        {
            if (this.BusyIndicator != null)
            {
                this.BusyIndicator.IsVisible = value;
                this.BusyIndicator.IsRunning = value;
            }
        }

        /// <summary>
        /// Changes the title of the data form to the specified new title.
        /// </summary>
        /// <param name="newTitle">The string value.</param>
        private void ChangeDataFormTitle(string newTitle)
        {
            this.DataFormNameLabel!.Text = newTitle;
        }

        /// <summary>
        /// Adds a message to the data form generator model's messages after a delay, optionally including suggestions.
        /// </summary>
        /// <param name="text">The string value.</param>
        /// <param name="suggestions">The assist item suggestion.</param>
        /// <returns>The assist view message.</returns>
        private async Task AddMessageWithDelayAsync(string text, AssistItemSuggestion? suggestions = null)
        {
            AssistItem assistItem = new AssistItem() { Text = text, Suggestion = suggestions!, ShowAssistItemFooter = false };
            await Task.Delay(1000).ConfigureAwait(true);
            this.DataFormGeneratorModel?.Messages.Add(assistItem);
        }

        /// <summary>
        /// Generates contact form suggestions for the assist view.
        /// </summary>
        /// <returns>The assist item suggestion.</returns>
        private AssistItemSuggestion GetContactFormSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = this.contactFormActions[0] },
                new AssistSuggestion() { Text = this.contactFormActions[1] },
                new AssistSuggestion() { Text = this.contactFormActions[2] },
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Generates employee details suggestions for the assist view.
        /// </summary>
        /// <returns>The assist item suggestion.</returns>
        private AssistItemSuggestion GetEmployeeDetailsSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = this.employeeDetailsActions[0] },
                new AssistSuggestion() { Text = this.employeeDetailsActions[1] },
                new AssistSuggestion() { Text = this.employeeDetailsActions[2] },
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Generates feedback form suggestions for the assist view.
        /// </summary>
        /// <returns>The assist item suggestion.</returns>
        private AssistItemSuggestion GetFeedbackFormSuggestion()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = this.feedbackFormActions[0] },
                new AssistSuggestion() { Text = this.feedbackFormActions[1] },
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Generates yes or no suggestions for the assist view.
        /// </summary>
        /// <returns>The assist item suggestion.</returns>
        private AssistItemSuggestion GetYesOrNoSuggestions()
        {
            var chatSubjectSuggestions = new AssistItemSuggestion();
            var contactSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion() { Text = this.yesOrNoSuggestions[0] },
                new AssistSuggestion() { Text = this.yesOrNoSuggestions[1] },
                new AssistSuggestion() { Text = this.yesOrNoSuggestions[2] },
            };
            chatSubjectSuggestions.Items = contactSuggestions;
            return chatSubjectSuggestions;
        }

        /// <summary>
        /// Generates a data form based on the user's prompt using AI, updating the data form and related UI elements accordingly.
        /// </summary>
        /// <param name="userPrompt">The string value prompt.</param>
        private async void GenerateAIDataForm(string userPrompt)
        {
            string dataFormNamePrompt = $"Generate a title for a data form based on the following string: {userPrompt}. The title should clearly reflect the purpose of the data form in general term. Provide only the title, with no additional explanation";
            string getDataFormName = await this.azureOpenAIBaseService.GetAIResponse(dataFormNamePrompt);
            this.DataFormNameLabel!.Text = getDataFormName;

            string prompt = $"Generate a data form based on the user prompt: {userPrompt}.";
            string condition = "Property names must be in PascalCase. " +
            "Must be property names and its value " +
            "Without additional formatting characters like backticks, newlines, or extra spaces. " +
            "and map each property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
            "The result must be in JSON format" +
            "Without additional formatting characters like backticks, newlines, or extra spaces.";

            var typeResponse = await this.azureOpenAIBaseService.GetAIResponse(prompt + condition);
            var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);

            if (this.DataForm != null && dataFormTypes != null)
            {
                var items = new ObservableCollection<DataFormViewItem>();
                foreach (var data in dataFormTypes)
                {
                    DataFormItem? dataFormItem = this.GenerateDataFormItems(data.Value.ToString(), data.Key);
                    if (dataFormItem != null)
                    {
                        items.Add(dataFormItem);
                    }
                }

                this.DataForm.Items = items;
            }

            AssistItem subjectMessage = new AssistItem() { Text = "As per your comment data form created successfully...", ShowAssistItemFooter = false };
            this.DataFormGeneratorModel?.Messages.Add(subjectMessage);

            this.UpdateCreateVisibility();
            this.UpdateBusyIndicator(false);
        }

        /// <summary>
        /// Updates the visibility of the create button and data form view based on the current state of the data form generator model.
        /// </summary>
        private void UpdateCreateVisibility()
        {
            if (this.DataFormGeneratorModel != null)
            {
                this.DataFormGeneratorModel.ShowInputView = false;
                this.DataFormGeneratorModel.ShowDataForm = true;
            }
        }

        /// <summary>
        /// Edits the existing data form based on the user's prompt and specified action, updating the form accordingly.
        /// </summary>
        /// <param name="userPrompt">The string value prompt.</param>
        /// <param name="action">The string value.</param>
        private async void EditDataForm(string userPrompt, string action)
        {
            if (this.DataForm!.Items == null)
            {
                return;
            }

            userPrompt = userPrompt.Replace(action, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            if (action == "Add")
            {
                string prompt = $"Generate a Property name based on the user prompt: {userPrompt}.";
                string condition = "The result must be in string" +
                "Property name must be in PascalCase, without asking questions, or including extra explanations. " +
                "Without additional formatting characters like backticks, newlines, or extra spaces." +
                $" and map that property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
                "The result must be in JSON format" +
                "Without additional formatting characters like backticks, newlines, or extra spaces.";

                var typeResponse = await this.azureOpenAIBaseService.GetAIResponse(prompt + condition);
                var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);

                if (dataFormTypes != null && dataFormTypes.Count > 0)
                {
                    var newItem = dataFormTypes.FirstOrDefault();
                    if (newItem.Value != null)
                    {
                        DataFormItem? dataFormItem = this.GenerateDataFormItems(newItem.Value.ToString(), newItem.Key);
                        if (dataFormItem != null)
                        {
                            this.DataForm.Items.Add(dataFormItem);
                            this.UpdateChangesResponse();
                        }
                    }
                }
            }
            else if (action == "Remove")
            {
                string prompt = $"Generate a Property name based on the user prompt: {userPrompt}.";
                string condition = "The result must be in string" +
                "Property name must be in PascalCase, without asking questions, or including extra explanations. " +
                "Without additional formatting characters like backticks, newlines, or extra spaces.";

                string response = await this.azureOpenAIBaseService.GetAIResponse(prompt + condition);
                var removeItem = this.DataForm!.Items.FirstOrDefault(x => x is DataFormItem dfi && dfi.FieldName == response);
                if (removeItem != null)
                {
                    this.DataForm.Items.Remove(removeItem);
                    this.UpdateChangesResponse();
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

                    string response = await this.azureOpenAIBaseService.GetAIResponse(prompt + condition);

                    string prompt1 = $"Generate a Property name from {match.Groups[3].Value.Trim()}.";
                    string condition1 = "The result must be in string and Property name must be in PascalCase,  without asking questions, or including extra explanations. " +
                    "Without additional formatting characters like backticks, newlines, or extra spaces." +
                    " map that generated property to the most appropriate DataForm available item type includes: DataFormTextItem , DataFormMultiLineTextItem, DataFormPasswordItem, DataFormNumericItem, DataFormMaskedTextItem, DataFormDateItem, DataFormTimeItem, DataFormCheckBoxItem, DataFormSwitchItem, DataFormPickerItem, DataFormComboBoxItem, DataFormAutoCompleteItem, DataFormRadioGroupItem, DataFormSegmentItem" +
                    "The result must be in JSON format" +
                    "without extra formatting like backticks, newlines, or special characters.";

                    var typeResponse = await this.azureOpenAIBaseService.GetAIResponse(prompt1 + condition1);
                    var dataFormTypes = JsonConvert.DeserializeObject<Dictionary<string, object>>(typeResponse);

                    if (dataFormTypes != null && !string.IsNullOrEmpty(response) && dataFormTypes.Count > 0)
                    {
                        var newItem = dataFormTypes.FirstOrDefault();
                        var removeItem = this.DataForm.Items.FirstOrDefault(x => x is DataFormItem dfi && dfi.FieldName == response);
                        if (removeItem != null && newItem.Value != null)
                        {
                            int index = this.DataForm.Items.IndexOf(removeItem);
                            var replaceItem = this.GenerateDataFormItems(newItem.Value.ToString(), newItem.Key);
                            if (replaceItem != null)
                            {
                                this.DataForm.Items[index] = replaceItem;
                            }

                            this.UpdateChangesResponse();
                        }
                    }
                }
            }
            else if (action == "Add Values" || action == "Add Value")
            {
                string prompt = $"Generate a Property name and a Values based on the user prompt: {userPrompt}." +
                " Output the result in the exact format: 'PropertyName: PropertyName, Values: value1, value2, ...'.";

                string condition = "The PropertyName must be in PascalCase, without extra questions, explanations, or formatting characters.";
                string response = await this.azureOpenAIBaseService.GetAIResponse(prompt + condition);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    var match = Regex.Match(response, @"PropertyName: (?<propertyName>[^,]+), Values: (?<values>.+)");
                    if (match.Success)
                    {
                        string propertyName = match.Groups["propertyName"].Value.Trim();
                        List<string> itemsSource = match.Groups["values"].Value
                            .Split(',')
                            .Select(v => v.Trim())
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .ToList();

                        var item = this.DataForm!.Items.FirstOrDefault(x => x is DataFormItem dfi && dfi.FieldName.Equals(propertyName));
                        if (item is DataFormListItem listItem)
                        {
                            listItem.ItemsSource = itemsSource;
                            this.UpdateChangesResponse();
                        }
                    }
                }
            }
            else if (action == "PlaceholderText")
            {
                // Example: "PlaceholderText for Email: Enter your work email"
                var match = Regex.Match(userPrompt, @"Placeholder(Text)?\s*(for)?\s*(?<field>\w+)\s*:\s*(?<text>.+)", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    var firstTextItem = this.DataForm.Items.OfType<DataFormTextItem>().FirstOrDefault();
                    if (firstTextItem != null)
                    {
                        firstTextItem.PlaceholderText = userPrompt;
                        this.UpdateChangesResponse();
                    }

                    return;
                }

                string fieldName = match.Groups["field"].Value.Trim();
                string placeholder = match.Groups["text"].Value.Trim();

                var item = this.DataForm.Items.FirstOrDefault(x => x is DataFormItem di && di.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                switch (item)
                {
                    case DataFormTextItem t:
                        t.PlaceholderText = placeholder;
                        this.UpdateChangesResponse();
                        break;
                    case DataFormMaskedTextItem m:
                        m.PlaceholderText = placeholder;
                        this.UpdateChangesResponse();
                        break;
                    case DataFormNumericItem n:
                        n.PlaceholderText = placeholder;
                        this.UpdateChangesResponse();
                        break;
                }
            }
            else
            {
                await this.AddMessageWithDelayAsync("No changes were made.");
                this.UpdateBusyIndicator(false);
            }
        }

        /// <summary>
        /// Updates the data form generator model with a message indicating that modifications have been completed successfully.
        /// </summary>
        private void UpdateChangesResponse()
        {
            AssistItem subjectMessage = new AssistItem() { Text = "Your modifications have been completed successfully....", ShowAssistItemFooter = false };
            this.DataFormGeneratorModel?.Messages.Add(subjectMessage);
        }

        /// <summary>
        /// Generates a DataFormItem based on the specified type and field name.
        /// </summary>
        /// <param name="dataFormItemType">The dataform type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The dataform item.</returns>
        private DataFormItem? GenerateDataFormItems(string? dataFormItemType, string fieldName)
        {
            DataFormItem? dataFormItem = dataFormItemType switch
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
                "DataFormAutoCompleteItem" => new DataFormAutoCompleteItem(),
                "DataFormRadioGroupItem" => new DataFormRadioGroupItem(),
                "DataFormComboBoxItem" => new DataFormComboBoxItem(),
                "DataFormPickerItem" => new DataFormPickerItem(),
                "DataFormSegmentItem" => new DataFormSegmentItem(),
                _ => null
            };

            if (dataFormItem == null)
            {
                return null;
            }

            dataFormItem.FieldName = fieldName;
            dataFormItem.LabelText = this.GetLabelText(fieldName);

            if (fieldName.Equals("Email", StringComparison.OrdinalIgnoreCase) && dataFormItem is DataFormTextItem emailEditor)
            {
                emailEditor.Keyboard = Keyboard.Email;
            }

            if (dataFormItem is DataFormListItem listItem)
            {
                if (!HasAnyItems(listItem.ItemsSource))
                {
                    listItem.ItemsSource = this.GetDummyItems(fieldName);
                }
            }

            return dataFormItem;
        }

        /// <summary>
        /// Provides dummy items for specific field names to populate list-based data form items.
        /// </summary>
        /// <param name="fieldName">The string field name.</param>
        /// <returns>The string value.</returns>
        private IEnumerable<string> GetDummyItems(string fieldName)
        {
            switch (fieldName)
            {
                case "Gender":
                case "Sex":
                    return new List<string> { "Male", "Female", "Other" };
                case "Country":
                    return new List<string> { "United States", "Canada", "United Kingdom", "Australia", "Germany" };
                case "State":
                    return new List<string> { "Alabama", "Alaska", "California", "Florida", "New York" };
                case "Department":
                    return new List<string> { "Sales", "Marketing", "Engineering", "HR", "Finance" };
                case "Priority":
                    return new List<string> { "Low", "Medium", "High", "Critical" };
                case "Status":
                    return new List<string> { "New", "In Progress", "On Hold", "Resolved", "Closed" };
                default:
                    return new List<string> { "Option 1", "Option 2", "Option 3" };
            }
        }

        /// <summary>
        /// Converts a field name in PascalCase to a more readable label text by inserting spaces before capital letters.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns>String value.</returns>
        private string GetLabelText(string fieldName)
        {
            return Regex.Replace(fieldName, "(\\B[A-Z])", " $1");
        }
    }
}