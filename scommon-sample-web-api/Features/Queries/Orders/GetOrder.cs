using Microsoft.EntityFrameworkCore;
using scommon;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Domains.Orders;

namespace scommon_sample_web_api.Features.Queries.Orders;

public static class GetOrder
{
    public class GetOrderQuery : Query<FeatureObjectResultModel<GetOrderResponse>>
    {
        public string? OrderNo { get; set; }
    }

    public class GetOrderResponse
    {
        public Guid Id { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? OrderNo { get; set; }

        public List<GetOrderResponseItem> Details { get; set; } = new();

        public class GetOrderResponseItem
        {
            public Guid Id { get; set; }
            public string? Product { get; set; }
            public decimal Price { get; set; }
        }
    }


    public class GetOrderHandler : IQueryHandler<GetOrderQuery, FeatureObjectResultModel<GetOrderResponse>>
    {
        private readonly SampleContext _dbContext;

        public GetOrderHandler(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FeatureObjectResultModel<GetOrderResponse>> HandleAsync(GetOrderQuery query, CancellationToken ct)
        {
            var order = await _dbContext.Set<Order>().Where(s => s.OrderNo == query.OrderNo).Include(s => s.OrderDetails).FirstOrDefaultAsync(ct);
            if (order is null)
            {
                return FeatureObjectResultModel<GetOrderResponse>.NotFound();
            }

            return FeatureObjectResultModel<GetOrderResponse>.Ok(new GetOrderResponse
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                OrderNo = order.OrderNo,
                Details = order.OrderDetails.Select(v => new GetOrderResponse.GetOrderResponseItem()
                {
                    Id = v.Id,
                    Product = v.Product,
                    Price = v.Price
                }).ToList()
            });
        }
    }
}
