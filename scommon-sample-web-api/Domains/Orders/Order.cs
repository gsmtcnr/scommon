using scommon;
using scommon_sample_web_api.Domains.Orders.OrderDetails;
using scommon.Utils.Constants;
using scommon.Utils.Extensions;

namespace scommon_sample_web_api.Domains.Orders;

public class Order : BaseUserTrackModel
{
    private Order()
    {
        OrderDetails = new List<OrderDetail>();
    }

    #region Properties

    public string? OrderNo { get; private set; }
    public decimal TotalPrice { get; private set; }

    #endregion

    #region Collections

    public ICollection<OrderDetail> OrderDetails { get; set; }

    #endregion

    #region Methods

    public static ResultDomain<Order> Create(string orderNo)
    {
        if (string.IsNullOrEmpty(orderNo))
        {
            return ResultDomain<Order>.Error(new MessageItem
            {
                Code = CommonResourceConstants.COMMON_MESSAGE_VALUE_EMPTY,
                Property = nameof(OrderNo),
                Table = nameof(Order),
            });
        }

        if (orderNo.Length > 32)
        {
            return ResultDomain<Order>.Error(new MessageItem
            {
                Code = CommonResourceConstants.COMMON_MESSAGE_VALUE_MAX_LENGHT_ERROR,
                Table = nameof(Order),
                Property = nameof(OrderNo),
                Params = new List<string> { "32" }
            });
        }

        var order = new Order
        {
            OrderNo = orderNo
        };

        return ResultDomain<Order>.Ok(order);
    }

    public ResultDomain<OrderDetail> AddDetail(string product, decimal price)
    {
        var orderDetailResult = OrderDetail.Create(Id, product, price);
        if (!orderDetailResult.IsSuccess) return ResultDomain<OrderDetail>.Error(orderDetailResult.Messages!);
        
        OrderDetails.Add(orderDetailResult.Data!);
        CalcTotalPrice();
        
        return ResultDomain<OrderDetail>.Ok(orderDetailResult.Data!);
    }

    public ResultDomain RemoveDetail(Guid id)
    {
        var orderDetail = OrderDetails.FirstOrDefault(s => s.Id == id);
        if (orderDetail is null) return ResultDomain.Error();

        OrderDetails.Remove(orderDetail);
        
        CalcTotalPrice();

        return ResultDomain.Ok();
    }

    private void CalcTotalPrice()
    {
        TotalPrice = OrderDetails.Sum(s => s.Price).ToRound();
    }

    #endregion
}
