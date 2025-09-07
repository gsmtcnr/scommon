using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using scommon;
using scommon_sample_web_api.Utils.Constants;

namespace scommon_sample_web_api.Domains.Orders.OrderDetails;

public class OrderDetailConfiguration : BaseConfiguration<OrderDetail>
{
    public override void Map(EntityTypeBuilder<OrderDetail> model)
    {
        model.HasOne(sq => sq.Order).WithMany(s => s.OrderDetails).HasForeignKey(sq => sq.OrderId).OnDelete(DeleteBehavior.Restrict).IsRequired();

        model.Property(m => m.Product).HasMaxLength(256);
        base.Map(model);
    }

    public override string GetSchemaName()
    {
        return SchemaConstants.ORDER_SCHEMA_NAME;
    }

    public override string GetTableName()
    {
        return nameof(OrderDetail);
    }
}
