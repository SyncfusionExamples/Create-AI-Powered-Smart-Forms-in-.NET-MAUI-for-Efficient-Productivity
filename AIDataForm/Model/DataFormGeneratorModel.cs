namespace AIDataForm
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// The DataForm generator model class.
    /// </summary>
    public class DataFormGeneratorModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds the azure AI services.
        /// </summary>
        private AzureAIServices azureAIServices = new AzureAIServices();

        /// <summary>
        /// Collection of messages in a conversation.
        /// </summary>
        private ObservableCollection<object> messages;

        /// <summary>
        /// Holds the showHeader value.
        /// </summary>
        private bool showHeader = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataFormGeneratorModel"/> class.
        /// </summary>
        public DataFormGeneratorModel()
        {
            this.messages = new ObservableCollection<object>();
        }

        /// <summary>
        /// Gets or sets the collection of messages of a conversation.
        /// </summary>
        public ObservableCollection<object> Messages
        {
            get
            {
                return this.messages;
            }

            set
            {
                this.messages = value;
            }
        }

        /// <summary>
        /// Gets or sets the show header.
        /// </summary>
        public bool ShowHeader
        {
            get { return this.showHeader; }
            set { this.showHeader = value; RaisePropertyChanged("ShowHeader"); }
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
