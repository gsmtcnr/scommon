using Microsoft.AspNetCore.Mvc;
using scommon;
using scommon_sample_web_api.Features.Commands.Orders;
using scommon_sample_web_api.Features.Commands.Orders.OrderDetails;
using scommon_sample_web_api.Features.Queries.Orders;

namespace scommon_sample_web_api.Controllers.Orders;

[Route("orders")]
public class OrdersController : BaseApiController
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Orders

    [HttpPost]
    [Route("")]
    public async Task<FeatureObjectResultModel<NewOrder.NewOrderResponse>> CreateOrder([FromBody] NewOrder.NewOrderCommand command, CancellationToken ct)
    {
        var result = await _mediator.SendAsync(command, ct);

        return result;
    }

    [HttpGet]
    [Route("")]
    public async Task<FeaturePagedResultModel<GetOrders.GetOrdersResponse>> GetOrders
        (int pageNumber, int pageSize, string? sortColumn, string? searchText, bool sortDescending, CancellationToken ct)
    {
        var result = await _mediator.FetchAsync(new GetOrders.GetOrdersQuery()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortDescending = sortDescending,
            SearchText = searchText,
            SortColumn = sortColumn
        }, ct);

        return result;
    }

    [HttpGet]
    [Route("{orderNo}")]
    public async Task<FeatureObjectResultModel<GetOrder.GetOrderResponse>> GetOrder
        (string orderNo, CancellationToken ct)
    {
        var result = await _mediator.FetchAsync(new GetOrder.GetOrderQuery()
        {
            OrderNo = orderNo
        }, ct);

        return result;
    }

    #endregion

    #region Order Details

    [HttpPost]
    [Route("{orderNo}/details")]
    public async Task<FeatureObjectResultModel<AddDetail.AddDetailResponse>> CreateOrderDetail(string orderNo, [FromBody] AddDetail.AddDetailCommand command, CancellationToken ct)
    {
        command.OrderNo = orderNo;
        var result = await _mediator.SendAsync(command, ct);

        return result;
    }

    [HttpDelete]
    [Route("{orderNo}/details")]
    public async Task<FeatureResultModel> DeleteOrderDetail(string orderNo, [FromBody] DeleteDetail.DeleteDetailCommand command, CancellationToken ct)
    {
        command.OrderNo = orderNo;
        var result = await _mediator.SendAsync(command, ct);

        return result;
    }

    #endregion
}
