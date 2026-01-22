namespace AIDataForm;

public partial class DataFormMobileUI : ContentView
{
	public DataFormMobileUI()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Handles the Tapped event of the TapGestureRecognizer control.
    /// </summary>
    /// <param name="sender">The sender value.</param>
    /// <param name="e">The event args.</param>
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var templateItem = (sender as Border)?.BindingContext as TemplateItem;
        var bindingContext = this.ParentGrid.BindingContext as DataFormGeneratorModel;

        if (templateItem != null && bindingContext != null)
        {
            entry.Text = templateItem.Description;
            bindingContext.FormTitle = templateItem.Title;
        }
    }

}