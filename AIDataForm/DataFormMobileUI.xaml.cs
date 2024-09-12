namespace AIDataForm;

public partial class DataFormMobileUI : ContentView
{
	public DataFormMobileUI()
	{
		InitializeComponent();
	}

    private void createButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(this.entry.Text))
        {
            return;
        }

        this.entry.IsVisible = false;
        this.createButton.IsVisible = false;
        this.mainGrid.IsVisible = true;
        this.aiActionButton.IsVisible = true;
    }

    private void aiActionButton_Clicked(object sender, EventArgs e)
    {
        this.aiPopupWindow.IsVisible = true;
        this.aiPopupWindow.IsOpen = true;
    }
}