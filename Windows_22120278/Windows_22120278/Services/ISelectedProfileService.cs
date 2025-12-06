using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface ISelectedProfileService
    {
        Profile? SelectedProfile { get; set; }
        event System.EventHandler<Profile?>? SelectedProfileChanged;
    }
}

