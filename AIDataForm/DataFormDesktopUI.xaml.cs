namespace AIDataForm;

public partial class DataFormDesktopUI : ContentView
{
	public DataFormDesktopUI()
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
    }
}