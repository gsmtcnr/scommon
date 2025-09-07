namespace scommon
{
    public interface IEntityConfiguration : ITransientDependency
    {
        string GetTableName();
        string GetSchemaName();
    }
}
