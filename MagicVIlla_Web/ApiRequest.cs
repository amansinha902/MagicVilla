using static MagicVilla_Utility.SD;

namespace MagicVIlla_Web
{
    public class ApiRequest
    {
        public ApiType apiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
    }
}
