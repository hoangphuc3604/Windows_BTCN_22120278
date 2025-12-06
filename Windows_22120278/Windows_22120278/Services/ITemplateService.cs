using System.Collections.Generic;
using System.Threading.Tasks;
using Windows_22120278.Models;
using Windows_22120278_Data.models;

namespace Windows_22120278.Services
{
    public interface ITemplateService
    {
        Task<List<ShapeTemplate>> GetAllTemplatesAsync(int profileId);
        Task<ShapeTemplate> SaveTemplateAsync(string name, DrawingShape shape, int profileId);
        Task<bool> DeleteTemplateAsync(int templateId);
        DrawingShape? ConvertTemplateToDrawingShape(ShapeTemplate template);
    }
}


