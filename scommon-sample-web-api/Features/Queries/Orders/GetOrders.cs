using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using scommon;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Domains.Orders;
using scommon.Utils.Helpers;

namespace scommon_sample_web_api.Features.Queries.Orders;

public static class GetOrders
{
    public class GetOrdersQuery : Query<FeaturePagedResultModel<GetOrdersResponse>>
    {
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public required bool SortDescending { get; set; }
        public string? SortColumn { get; set; }
        public string? SearchText { get; set; }
    }

    public class GetOrdersResponse
    {
        public Guid Id { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? OrderNo { get; set; }
    }


    public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, FeaturePagedResultModel<GetOrdersResponse>>
    {
        private readonly SampleContext _dbContext;

        public GetOrdersHandler(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<FeaturePagedResultModel<GetOrdersResponse>> HandleAsync(GetOrdersQuery query, CancellationToken ct)
        {
            var ordersQuery = _dbContext.Set<Order>().AsNoTracking().AsQueryable();

            var pagerInputModel = PagerInputModel.Create(query.PageNumber, query.PageSize, query.SortColumn!, query.SearchText!, query.SortDescending);

            if (!string.IsNullOrEmpty(pagerInputModel.SortColumn))
            {
                ordersQuery = pagerInputModel.SortDescending ? ordersQuery.OrderByDescending(pagerInputModel.SortColumn) : ordersQuery.OrderBy(pagerInputModel.SortColumn);
            }

            if (!string.IsNullOrEmpty(pagerInputModel.SearchText))
            {
                ordersQuery = ordersQuery.Where(s => s.OrderNo!.Contains(pagerInputModel.SearchText));
            }

            var pagedResult = ordersQuery.ToPagedList(pagerInputModel.PageNumber, pagerInputModel.PageSize);

            var items = pagedResult.Select(s => new GetOrdersResponse
            {
                Id = s.Id,
                TotalPrice = s.TotalPrice,
                OrderNo = s.OrderNo
            }).ToList();

            return Task.FromResult(FeaturePagedResultModel<GetOrdersResponse>.Ok(pagedResult, items));
        }
    }
}
