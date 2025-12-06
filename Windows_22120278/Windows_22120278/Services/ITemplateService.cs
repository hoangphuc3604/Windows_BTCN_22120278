using System.Collections.Generic;
using System.Threading.Tasks;
using Windows_22120278.Models;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface ITemplateService
    {
        Task<List<ShapeTemplate>> GetAllTemplatesAsync();
        Task<ShapeTemplate> SaveTemplateAsync(string name, DrawingShape shape);
        Task<bool> DeleteTemplateAsync(int templateId);
        DrawingShape? ConvertTemplateToDrawingShape(ShapeTemplate template);
    }
}


