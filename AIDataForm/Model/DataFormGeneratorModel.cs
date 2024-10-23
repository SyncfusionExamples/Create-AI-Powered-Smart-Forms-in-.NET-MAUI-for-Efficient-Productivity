namespace AIDataForm
{
    using Syncfusion.Maui.AIAssistView;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// The DataForm generator model class.
    /// </summary>
    public class DataFormGeneratorModel : INotifyPropertyChanged
    {
        private bool showDataForm , showAssistView, showSubmitButton, showInputView, showOfflineLabel;
       
        public DataFormGeneratorModel()
        {
            showInputView = true;
            showOfflineLabel = true;
        }
        /// <summary>
        /// Gets or sets the collection of messages of a conversation.
        /// </summary>
        public ObservableCollection<IAssistItem> Messages { get; set; } = new ObservableCollection<IAssistItem>();
       

        /// <summary>
        /// Gets or sets the show header.
        /// </summary>
        public bool ShowDataForm
        {
            get { return this.showDataForm; }
            set
            {
                this.showDataForm = value;
                RaisePropertyChanged(nameof(this.ShowDataForm));
            }
        }

        public bool ShowAssistView
        {
            get { return this.showAssistView; }
            set
            {
                this.showAssistView = value;
                RaisePropertyChanged(nameof(this.ShowAssistView));
            }
        }

        public bool ShowInputView
        {
            get { return this.showInputView; }
            set
            {
                this.showInputView = value;
                RaisePropertyChanged(nameof(this.ShowInputView));
            }
        }

        public bool ShowSubmitButton
        {
            get { return this.showSubmitButton; }
            set
            {
                this.showSubmitButton = value;
                RaisePropertyChanged(nameof(this.ShowSubmitButton));
            }
        }

        public bool ShowOfflineLabel
        {
            get { return this.showOfflineLabel; }
            set
            {
                this.showOfflineLabel = value;
                RaisePropertyChanged(nameof(this.ShowOfflineLabel));
            }
        }

        /// <summary>
        /// Property changed handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</param>
        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
