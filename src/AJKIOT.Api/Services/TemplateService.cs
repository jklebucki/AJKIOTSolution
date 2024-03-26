
namespace AJKIOT.Api.Services
{
    public class TemplateService : ITemplateService
    {
        public async Task<string> GetTemplateAsync(string name)
        {
            //implement file reading here
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "HtmlTemplates", $"{name}");
            if (File.Exists(filePath))
                return await File.ReadAllTextAsync(filePath);

            return string.Empty;

        }
    }
}
