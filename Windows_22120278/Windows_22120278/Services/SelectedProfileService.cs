using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public class SelectedProfileService : ISelectedProfileService
    {
        private Profile? _selectedProfile;

        public Profile? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile != value)
                {
                    _selectedProfile = value;
                    SelectedProfileChanged?.Invoke(this, value);
                }
            }
        }

        public event System.EventHandler<Profile?>? SelectedProfileChanged;
    }
}


