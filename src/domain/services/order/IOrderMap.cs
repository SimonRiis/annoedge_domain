using domain.order;
using LanguageExt;

namespace domain.services.order
{
    public interface IOrderMap
    {
        Fin<Order> MapFrom(OrderDTO order);
    }
}
