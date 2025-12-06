using Microsoft.UI.Xaml.Controls;
using Windows_22120278_Data.models;

namespace Windows_22120278.Views
{
    public sealed partial class EditProfileDialog : ContentDialog
    {
        private readonly Profile _originalProfile;

        public EditProfileDialog(Profile profile)
        {
            InitializeComponent();
            _originalProfile = profile;
            
            NameTextBox.Text = profile.Name;
            IsDarkThemeCheckBox.IsChecked = profile.IsDefaultThemeDark;
            WidthNumberBox.Value = profile.DefaultBoardWidth;
            HeightNumberBox.Value = profile.DefaultBoardHeight;
        }

        public Profile? GetUpdatedProfile()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                return null;

            return new Profile
            {
                Id = _originalProfile.Id,
                Name = NameTextBox.Text.Trim(),
                IsDefaultThemeDark = IsDarkThemeCheckBox.IsChecked ?? false,
                DefaultBoardWidth = WidthNumberBox.Value,
                DefaultBoardHeight = HeightNumberBox.Value
            };
        }
    }
}

