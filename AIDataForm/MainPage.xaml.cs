namespace AIDataForm
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
#if ANDROID || IOS
            this.Content = new DataFormMobileUI();
#else
            this.Content = new DataFormDesktopUI();
#endif
        }

    }

}
