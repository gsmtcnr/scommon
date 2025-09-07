using scommon;
using scommon.Utils.Constants;

namespace scommon_sample_web_api.Domains.Orders.OrderDetails;

public class OrderDetail : BaseUserTrackModel
{
    private OrderDetail()
    {
    }

    #region Relations

    public Guid OrderId { get; private set; }
    public Order? Order { get; set; }

    #endregion

    #region Properties

    public string? Product { get; private set; }
    public decimal Price { get; private set; }

    #endregion


    #region Methods

    public static ResultDomain<OrderDetail> Create(Guid orderId, string product, decimal price)
    {
        #region Validations

        if (orderId.Equals(Guid.Empty))
        {
            return ResultDomain<OrderDetail>.Error(new MessageItem
            {
                Code = CommonResourceConstants.COMMON_MESSAGE_VALUE_EMPTY,
                Property = nameof(Order),
                Table = nameof(OrderDetail),
            });
        }

        if (string.IsNullOrEmpty(product))
        {
            return ResultDomain<OrderDetail>.Error(new MessageItem
            {
                Code = CommonResourceConstants.COMMON_MESSAGE_VALUE_EMPTY,
                Property = nameof(Product),
                Table = nameof(OrderDetail),
            });
        }

        if (product.Length > 256)
        {
            return ResultDomain<OrderDetail>.Error(new MessageItem
            {
                Code = CommonResourceConstants.COMMON_MESSAGE_VALUE_MAX_LENGHT_ERROR,
                Table = nameof(OrderDetail),
                Property = nameof(Product),
                Params = new List<string> { "256" }
            });
        }

        #endregion

        var orderDetail = new OrderDetail
        {
            Product = product,
            Price = price
        };

        return ResultDomain<OrderDetail>.Ok(orderDetail);
    }

    #endregion
}
