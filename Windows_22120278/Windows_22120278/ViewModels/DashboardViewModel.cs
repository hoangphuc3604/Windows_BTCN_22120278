using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using Windows_22120278.Services;
using Windows_22120278_Data;
using Windows_22120278_Data.models;

namespace Windows_22120278.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _context;
        private readonly INavigationService _navigationService;
        private readonly ISelectedProfileService _selectedProfileService;

        [ObservableProperty]
        private int totalBoards;

        [ObservableProperty]
        private int totalTemplates;

        [ObservableProperty]
        private int totalShapes;

        [ObservableProperty]
        private ObservableCollection<ISeries> shapeDistribution = new();

        public DashboardViewModel(AppDbContext context, INavigationService navigationService, ISelectedProfileService selectedProfileService)
        {
            _context = context;
            _navigationService = navigationService;
            _selectedProfileService = selectedProfileService;
        }

        [RelayCommand]
        private async Task LoadStatsAsync()
        {
            var profile = _selectedProfileService.SelectedProfile;
            if (profile == null)
                return;

            var shapeGroups = await _context.Shapes
                .Where(s => s.DrawingBoard != null && s.DrawingBoard.ProfileId == profile.Id)
                .GroupBy(s => s.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            TotalBoards = await _context.DrawingBoards
                .Where(b => b.ProfileId == profile.Id)
                .CountAsync();

            TotalTemplates = await _context.ShapeTemplates
                .Where(t => t.ProfileId == profile.Id)
                .CountAsync();

            TotalShapes = await _context.Shapes
                .Where(s => s.DrawingBoard != null && s.DrawingBoard.ProfileId == profile.Id)
                .CountAsync();

            var series = new ObservableCollection<ISeries>();

            var colors = new[]
            {
                SKColor.Parse("#FF6384"),
                SKColor.Parse("#36A2EB"),
                SKColor.Parse("#FFCE56"),
                SKColor.Parse("#4BC0C0"),
                SKColor.Parse("#9966FF"),
                SKColor.Parse("#FF9F40")
            };

            int colorIndex = 0;
            foreach (var group in shapeGroups)
            {
                var seriesItem = new PieSeries<double>
                {
                    Values = new double[] { group.Count },
                    Name = group.Type.ToString(),
                    Fill = new SolidColorPaint(colors[colorIndex % colors.Length])
                };
                series.Add(seriesItem);
                colorIndex++;
            }

            ShapeDistribution = series;
        }

        [RelayCommand]
        private void NavigateToSavedCanvases()
        {
            _navigationService.NavigateTo("SavedCanvases");
        }

        [RelayCommand]
        private void NavigateToTemplates()
        {
            _navigationService.NavigateTo("Templates");
        }
    }
}

