using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Windows_22120278.Services;
using Windows_22120278.Views;
using Windows_22120278_Data.models;

namespace Windows_22120278.ViewModels
{
    public partial class BoardsViewModel : ObservableObject
    {
        private readonly IDrawingService _drawingService;
        private readonly ITemplateService _templateService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<DrawingBoard> boards = new();

        [ObservableProperty]
        private ObservableCollection<ShapeTemplate> templates = new();

        [ObservableProperty]
        private Profile? profile;

        public BoardsViewModel(IDrawingService drawingService, ITemplateService templateService, INavigationService navigationService)
        {
            _drawingService = drawingService;
            _templateService = templateService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task LoadBoardsAsync()
        {
            if (Profile == null)
                return;

            try
            {
                var boardsList = await _drawingService.GetDrawingBoardsByProfileIdAsync(Profile.Id);
                Boards.Clear();
                foreach (var board in boardsList)
                {
                    Boards.Add(board);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading boards: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadTemplatesAsync()
        {
            if (Profile == null)
                return;

            try
            {
                var templatesList = await _templateService.GetAllTemplatesAsync(Profile.Id);
                Templates.Clear();
                foreach (var template in templatesList)
                {
                    Templates.Add(template);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading templates: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CreateBoardAsync()
        {
            if (Profile == null)
                return;

            await Task.CompletedTask;
        }

        public async Task<bool> CreateBoardWithDialogAsync(double defaultWidth, double defaultHeight, Microsoft.UI.Xaml.XamlRoot xamlRoot)
        {
            try
            {
                var createDialog = new CreateBoardDialog(defaultWidth, defaultHeight, "#FFFFFF");
                createDialog.XamlRoot = xamlRoot;
                var result = await createDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var newBoard = new DrawingBoard
                    {
                        Name = createDialog.GetBoardName(),
                        Width = createDialog.GetWidth(),
                        Height = createDialog.GetHeight(),
                        BackgroundColor = createDialog.GetBackgroundColor(),
                        ProfileId = Profile!.Id
                    };

                    var createdBoard = await _drawingService.CreateDrawingBoardAsync(newBoard);
                    Boards.Add(createdBoard);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating board: {ex.Message}");
                return false;
            }
        }

        [RelayCommand]
        private async Task DeleteBoardAsync(DrawingBoard board)
        {
            if (board == null)
                return;

            try
            {
                var success = await _drawingService.DeleteDrawingBoardAsync(board.Id);
                if (success)
                {
                    Boards.Remove(board);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting board: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenDrawing(DrawingBoard board)
        {
            if (board == null)
                return;

            _navigationService.NavigateToDrawing(board);
        }

        [RelayCommand]
        private async Task DeleteTemplateAsync(ShapeTemplate template)
        {
            if (template == null)
                return;

            try
            {
                var success = await _templateService.DeleteTemplateAsync(template.Id);
                if (success)
                {
                    Templates.Remove(template);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting template: {ex.Message}");
            }
        }

        [RelayCommand]
        private void PreviewTemplate(ShapeTemplate template)
        {
            if (template == null)
                return;

            // This will be handled in the code-behind to show a preview dialog
            // The command is here for binding purposes
        }
    }
}

