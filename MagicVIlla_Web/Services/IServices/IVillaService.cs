using MagicVilla_Web.Dto;

namespace MagicVIlla_Web.Services.IServices
{
    public interface IVillaService
    {
        Task<T> GetAllAsync<T>();     
        Task<T> GetAsyncT<T>(int id);
        Task<T> CreateAsync<T>(VillaCreateDto dto);
        Task<T> UpdateAsync<T>(VillaUpdateDto dto);
        Task<T> DeleteAsync<T>(int id);
    }
}
