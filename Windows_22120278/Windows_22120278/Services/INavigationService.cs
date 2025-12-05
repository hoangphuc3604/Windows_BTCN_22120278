namespace Windows_22120278.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageName);
        void GoBack();
    }
}


