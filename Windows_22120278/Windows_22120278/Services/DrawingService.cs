using System.Threading.Tasks;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public class DrawingService : IDrawingService
    {
        public Task SaveDrawingAsync(DrawingBoard drawingBoard)
        {
            return Task.CompletedTask;
        }
    }
}

