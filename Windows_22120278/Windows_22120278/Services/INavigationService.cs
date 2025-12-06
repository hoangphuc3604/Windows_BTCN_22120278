using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageName, object? parameter = null);
        void NavigateToDrawing(DrawingBoard board);
        void NavigateToBoards(Profile profile);
        void GoBack();
        void SetFrame(Microsoft.UI.Xaml.Controls.Frame frame);
    }
}


