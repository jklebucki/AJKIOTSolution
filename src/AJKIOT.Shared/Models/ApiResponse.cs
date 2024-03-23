namespace AJKIOT.Shared.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess => !Errors.Any();
    }
}
