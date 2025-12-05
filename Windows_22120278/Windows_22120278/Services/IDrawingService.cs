using System.Threading.Tasks;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface IDrawingService
    {
        Task SaveDrawingAsync(DrawingBoard drawingBoard);
    }
}


