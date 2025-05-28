using Server.API.Common;
using Server.API.Models;

namespace Server.API.Repositories
{
    public interface IProductRepository
    {
        Task<StateResponseModel<List<Products>>> getAll();
        Task<StateResponseModel<string>> reset();
    }
}
