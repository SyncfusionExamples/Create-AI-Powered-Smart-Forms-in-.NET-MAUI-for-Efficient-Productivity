namespace AIDataForm
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// The DataForm generator model class.
    /// </summary>
    public class DataFormGeneratorModel : INotifyPropertyChanged
    {

        private bool showDataForm;

        /// <summary>
        /// Gets or sets the collection of messages of a conversation.
        /// </summary>
        public ObservableCollection<object> Messages { get; set; } = new ObservableCollection<object>();

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
