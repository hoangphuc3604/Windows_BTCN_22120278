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
using Windows_22120278_Data;
using Windows_22120278_Data.models;

namespace Windows_22120278.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _context;

        [ObservableProperty]
        private int totalBoards;

        [ObservableProperty]
        private ObservableCollection<ISeries> shapeDistribution = new();

        public DashboardViewModel(AppDbContext context)
        {
            _context = context;
        }

        [RelayCommand]
        private async Task LoadStatsAsync()
        {
            var shapeGroups = await _context.Shapes
                .GroupBy(s => s.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            TotalBoards = await _context.DrawingBoards.Distinct().CountAsync();

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
    }
}

