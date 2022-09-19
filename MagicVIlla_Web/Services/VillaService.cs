using MagicVilla_Utility;
using MagicVilla_Web.Dto;
using MagicVIlla_Web.Services.IServices;

namespace MagicVIlla_Web.Services
{
    public class VillaService : BaseService, IVillaService
    {
        private readonly IHttpClientFactory _clientFactory;
        private  string _villaUrl;

        public VillaService(IHttpClientFactory clientFactory,IConfiguration configuration) : base(clientFactory) 
        {
            _clientFactory = clientFactory;
            _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public Task<T> CreateAsync<T>(VillaCreateDto dto)
        {
            return SendAsync<T>(new ApiRequest()
            {
                apiType = SD.ApiType.POST,
                Data = dto,
                Url = _villaUrl + "/api/VillaApi"
            });
        }

        public Task<T> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest()
            {
                apiType = SD.ApiType.DELETE,
                Url = _villaUrl + "/api/VillaApi/" + id
            }) ;
        }

        public Task<T> GetAsyncT<T>(int id)
        {
            return SendAsync<T>(new ApiRequest()
            {
                apiType = SD.ApiType.GET,
                Url = _villaUrl + "/api/VillaApi/" + id
            });
        }

        public Task<T> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                apiType = SD.ApiType.GET,
           
                Url = _villaUrl + "/api/VillaApi"
            });
        }

        public Task<T> UpdateAsync<T>(VillaUpdateDto dto)
        {
            return SendAsync<T>(new ApiRequest()
            {
                apiType = SD.ApiType.PUT,
                Data = dto,
                Url = _villaUrl + "/api/VillaApi/" +dto.Id
            });
        }
    }
}
