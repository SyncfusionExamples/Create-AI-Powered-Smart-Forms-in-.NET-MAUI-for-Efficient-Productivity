namespace AIDataForm
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Syncfusion.Maui.AIAssistView;

    /// <summary>
    /// DataForm Generator Model.
    /// </summary>
    public class DataFormGeneratorModel : INotifyPropertyChanged
    {
        private bool showDataForm;
        private bool showAssistView;
        private bool showSubmitButton;
        private bool showInputView;
        private bool showOfflineLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFormGeneratorModel"/> class.
        /// </summary>
        public DataFormGeneratorModel()
        {
            this.showInputView = true;
            this.showOfflineLabel = true;

            this.Templates = new ObservableCollection<TemplateItem>
            {
                new TemplateItem { Font = "\ue763", Title = "Contact Form", Description = "Create a form to capture user details." },
                new TemplateItem { Font = "\ue761", Title = "Employment Details", Description = "Create a form to capture employment details." },
                new TemplateItem { Font = "\ue73a", Title = "Feedback Form", Description = "Create a form to receive client feedback." },
            };
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the templates.
        /// </summary>
        public ObservableCollection<TemplateItem> Templates { get; set; }

        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        public ObservableCollection<IAssistItem> Messages { get; set; } = new ObservableCollection<IAssistItem>();

        /// <summary>
        /// Gets or sets a value indicating whether to show data form.
        /// </summary>
        public bool ShowDataForm
        {
            get => this.showDataForm;
            set
            {
                this.showDataForm = value;
                this.RaisePropertyChanged(nameof(this.ShowDataForm));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show assist view.
        /// </summary>
        public bool ShowAssistView
        {
            get => this.showAssistView;
            set
            {
                this.showAssistView = value;
                this.RaisePropertyChanged(nameof(this.ShowAssistView));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show input view.
        /// </summary>
        public bool ShowInputView
        {
            get => this.showInputView;
            set
            {
                this.showInputView = value;
                this.RaisePropertyChanged(nameof(this.ShowInputView));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show submit button.
        /// </summary>
        public bool ShowSubmitButton
        {
            get => this.showSubmitButton;
            set
            {
                this.showSubmitButton = value;
                this.RaisePropertyChanged(nameof(this.ShowSubmitButton));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show offline label.
        /// </summary>
        public bool ShowOfflineLabel
        {
            get => this.showOfflineLabel;
            set
            {
                this.showOfflineLabel = value;
                this.RaisePropertyChanged(nameof(this.ShowOfflineLabel));
            }
        }

        /// <summary>
        /// Gets or sets the form title.
        /// </summary>
        internal string? FormTitle { get; set; }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propName">The string propname.</param>
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}