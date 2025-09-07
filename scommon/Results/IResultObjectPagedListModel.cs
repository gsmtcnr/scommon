
namespace scommon
{
    public interface IResultObjectPagedListModel<TData> : IResultObjectListModel<TData>, IResultPagedListModel
         where TData : class, new()
    {

    }
}
