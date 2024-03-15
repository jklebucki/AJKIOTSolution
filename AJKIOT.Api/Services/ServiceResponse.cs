namespace AJKIOT.Api.Services
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess => !Errors.Any();
    }
}
