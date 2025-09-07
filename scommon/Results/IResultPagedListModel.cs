﻿namespace scommon
{
    public interface IResultPagedListModel
    {
        int TotalItemCount { get; set; }
        int PageCount { get; set; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
