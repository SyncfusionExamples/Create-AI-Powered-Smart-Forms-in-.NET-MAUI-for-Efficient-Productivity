namespace AIDataForm
{
    using System;
    using Microsoft.Maui.Controls;
    using Syncfusion.Maui.Core;

    /// <summary>
    /// Interaction logic for DataFormDesktopUI.xaml.
    /// </summary>
    public partial class DataFormDesktopUI : ContentView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFormDesktopUI"/> class.
        /// </summary>
        public DataFormDesktopUI()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the Tapped event of the TapGestureRecognizer control.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The tapped event args.</param>
        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            var templateItem = (sender as Border)?.BindingContext as TemplateItem;
            var bindingContext = this.ParentGrid.BindingContext as DataFormGeneratorModel;
            if (templateItem != null && bindingContext != null)
            {
                this.entry.Text = templateItem.Description;
                bindingContext.FormTitle = templateItem.Title;
            }
        }
    }
}