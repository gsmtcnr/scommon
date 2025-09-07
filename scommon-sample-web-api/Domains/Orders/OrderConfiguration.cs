using Microsoft.EntityFrameworkCore.Metadata.Builders;
using scommon;
using scommon_sample_web_api.Utils.Constants;

namespace scommon_sample_web_api.Domains.Orders;

public class OrderConfiguration : BaseConfiguration<Order>
{
    public override void Map(EntityTypeBuilder<Order> model)
    {
        model.Property(m => m.OrderNo).HasMaxLength(32);
        base.Map(model);
    }

    public override string GetSchemaName()
    {
        return SchemaConstants.ORDER_SCHEMA_NAME;
    }

    public override string GetTableName()
    {
        return nameof(Order);
    }
}
