using FluentValidation;
using Microsoft.EntityFrameworkCore;
using scommon;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Domains.Orders;
using scommon.Utils.Constants;

namespace scommon_sample_web_api.Features.Commands.Orders;

public static class NewOrder
{
    public class NewOrderCommand : Command<FeatureObjectResultModel<NewOrderResponse>>
    {
        public string? OrderNo { get; set; }
    }

    public class NewOrderResponse
    {
        public Guid Id { get; set; }
    }

    #region Validations

    public class NewOrderCommandRequestValidation : AbstractValidator<NewOrderCommand>
    {
        public NewOrderCommandRequestValidation()
        {
            RuleFor(x => x.OrderNo).NotEmpty().Must(x => x.Length is >= 6 and <= 32).WithErrorCode(CommonResourceConstants.COMMON_MESSAGE_VALUE_MIN_LENGHT_ERROR);
        }
    }

    public class Validator : IValidationHandler<NewOrderCommand>
    {
        private readonly SampleContext _dbContext;

        public Validator(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IResultModel> ValidateAsync(NewOrderCommand cmd, CancellationToken cancellationToken)
        {
            var orderExists = await _dbContext.Set<Order>().AnyAsync(s =>
                s.OrderNo == cmd.OrderNo, cancellationToken);

            if (orderExists)
            {
                return FeatureResultModel.Error(new MessageItem
                {
                    Table = nameof(Order),
                    Property = nameof(Order.OrderNo),
                    Code = CommonResourceConstants.COMMON_MESSAGE_RECORD_DUPLICATE
                });
            }

            return FeatureResultModel.Ok();
        }
    }

    #endregion

    public class NewOrderHandler : ICommandHandler<NewOrderCommand, FeatureObjectResultModel<NewOrderResponse>>
    {
        private readonly SampleContext _dbContext;

        public NewOrderHandler(SampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FeatureObjectResultModel<NewOrderResponse>> HandleAsync(NewOrderCommand cmd, CancellationToken ct)
        {
            var orderResult = Order.Create(cmd.OrderNo!);

            if (!orderResult.IsSuccess)
            {
                return FeatureObjectResultModel<NewOrderResponse>.Error(orderResult.Messages);
            }

            await _dbContext.Set<Order>().AddAsync(orderResult.Data!, ct);

            await _dbContext.SaveChangesAsync(ct);//InMemoryDb için

            return FeatureObjectResultModel<NewOrderResponse>.Ok(new NewOrderResponse
            {
                Id = orderResult.Data!.Id
            });
        }
    }
}
