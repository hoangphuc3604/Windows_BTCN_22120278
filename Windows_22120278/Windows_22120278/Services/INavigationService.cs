namespace Windows_22120278.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageName, object? parameter = null);
        void GoBack();
        void SetFrame(Microsoft.UI.Xaml.Controls.Frame frame);
    }
}


