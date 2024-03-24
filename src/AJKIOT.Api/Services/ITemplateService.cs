namespace AJKIOT.Api.Services
{
    public interface ITemplateService
    {
        Task<string> GetTemplateAsync(string name);
    }
}
