using MagicVilla_VillaApi.Models;

namespace MagicVilla_VillaApi.Repository.IRepository
{
    public interface IVillaRepository : IRepository<Villa>
    {

        Task<Villa> UpdateAsync(Villa entity);


    }
}
