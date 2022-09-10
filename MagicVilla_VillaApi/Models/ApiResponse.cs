using System.Net;

namespace MagicVilla_VillaApi.Models
{
    public class ApiResponse // this is used to provide generic api response.
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<String> ErrorMessages { get; set; }
        public object Result { get; set; }
    }
}
