using Server.API.Common;
using Server.API.Sevices;

namespace Server.API.Repositories
{
    public interface ICartRepository
    {
        Task<StateResponseModel<List<CartItemDTO>>> getCart();
        Task<StateResponseModel<string>> updateCart(Guid productId, int? quantity, int? incrementBy);
        Task<StateResponseModel<string>> deleteCart(Guid producId);
        Task<StateResponseModel<string>> emptyCart();
    }
}
