﻿namespace scommon
{
    public abstract class BaseResultValueModel<TData> : BaseResultModel, IResultValueModel<TData>
    {
        public TData? Value { get; set; }
    }
}
