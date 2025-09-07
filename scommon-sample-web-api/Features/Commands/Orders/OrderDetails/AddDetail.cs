using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using scommon;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Domains.Orders;
using scommon_sample_web_api.Domains.Orders.OrderDetails;
using scommon.Utils.Constants;

namespace scommon_sample_web_api.Features.Commands.Orders.OrderDetails;

public static class AddDetail
{
    public class AddDetailCommand : Command<FeatureObjectResultModel<AddDetailResponse>>
    {
        [JsonIgnore]
        public string? OrderNo { get; set; }
        public string? Product { get; set; }
        public decimal Price { get; set; }
    }

    public class AddDetailResponse
    {
        public Guid Id { get; set; }
    }

    #region Validations

    public class AddDetailCommandRequestValidation : AbstractValidator<AddDetailCommand>
    {
        public AddDetailCommandRequestValidation()
        {
            RuleFor(x => x.OrderNo).NotEmpty().WithErrorCode(CommonResourceConstants.COMMON_MESSAGE_VALUE_EMPTY);
            RuleFor(x => x.Product).NotEmpty().WithErrorCode(CommonResourceConstants.COMMON_MESSAGE_VALUE_EMPTY);
        }
    }

    #endregion

    public class AddDetailHandler : ICommandHandler<AddDetailCommand, FeatureObjectResultModel<AddDetailResponse>>
    {
        private readonly SampleContext _dbContext;

        public AddDetailHandler(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FeatureObjectResultModel<AddDetailResponse>> HandleAsync(AddDetailCommand cmd, CancellationToken ct)
        {
            var order = await _dbContext.Set<Order>().Where(s => s.OrderNo == cmd.OrderNo).Include(s => s.OrderDetails).FirstOrDefaultAsync(ct);
            if (order is null)
            {
                return FeatureObjectResultModel<AddDetailResponse>.NotFound();
            }

            var addDetailResult = order.AddDetail(cmd.Product!, cmd.Price);
            if (!addDetailResult.IsSuccess)
            {
                return FeatureObjectResultModel<AddDetailResponse>.Error(addDetailResult.Messages);
            }
            await _dbContext.Set<OrderDetail>().AddAsync(addDetailResult.Data!, ct);

            
            await _dbContext.SaveChangesAsync(ct);//InMemoryDb için
            return FeatureObjectResultModel<AddDetailResponse>.Ok(new AddDetailResponse
            {
                Id = addDetailResult.Data!.Id
            });
        }
    }
}
