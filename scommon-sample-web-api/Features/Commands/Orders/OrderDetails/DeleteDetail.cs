using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using scommon;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Domains.Orders;

namespace scommon_sample_web_api.Features.Commands.Orders.OrderDetails;

public static class DeleteDetail
{
    public class DeleteDetailCommand : Command<FeatureResultModel>
    {
        [JsonIgnore]
        public string? OrderNo { get; set; }
        public required Guid Id { get; set; }
    }

    public class DeleteDetailHandler : ICommandHandler<DeleteDetailCommand, FeatureResultModel>
    {
        private readonly SampleContext _dbContext;

        public DeleteDetailHandler(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FeatureResultModel> HandleAsync(DeleteDetailCommand cmd, CancellationToken ct)
        {
            var order = await _dbContext.Set<Order>().Where(s => s.OrderNo == cmd.OrderNo).Include(s => s.OrderDetails).FirstOrDefaultAsync(ct);
            if (order is null)
            {
                return FeatureResultModel.NotFound();
            }

            var detailResult = order.RemoveDetail(cmd.Id);
            
            if (!detailResult.IsSuccess)
            {
                return FeatureResultModel.Error(detailResult.Messages);

            }
            await _dbContext.SaveChangesAsync(ct);//InMemoryDb için
            return FeatureResultModel.Ok();
        }
    }
}
