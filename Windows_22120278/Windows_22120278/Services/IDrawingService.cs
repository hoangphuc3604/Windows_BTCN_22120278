using System.Collections.Generic;
using System.Threading.Tasks;
using Windows_22120278.Models;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface IDrawingService
    {
        Task<DrawingBoard> SaveDrawingAsync(DrawingBoard drawingBoard, List<DrawingShape> shapes);
        Task<(DrawingBoard? board, List<DrawingShape> shapes)> GetDrawingAsync(int drawingBoardId);
        Task<DrawingBoard?> GetLatestDrawingBoardAsync(int profileId);
        Task<List<DrawingBoard>> GetDrawingBoardsByProfileIdAsync(int profileId);
        Task<DrawingBoard> CreateDrawingBoardAsync(DrawingBoard board);
        Task<bool> DeleteDrawingBoardAsync(int boardId);
    }
}


