namespace Ecommerce.Infrastructure.Responses
{
    public class ApiResponse<T>
    {
        public bool sucess { get; set; }

        public string message { get; set; } = string.Empty;

        public T? data { get; set; }






    }
}
