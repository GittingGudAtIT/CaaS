using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Common
{
    public class MayDenied<T>
    {
        public MayDenied(RequestResult accessResult, T? value = default)
        {
            RequestResult = accessResult;
            Value = value;
        }

        public RequestResult RequestResult { get; set; }
        public T? Value { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is MayDenied<T> den
                && (den.Value is null && Value is null
                    || den.Value is not null && den.Value.Equals(Value)
                )
                && den.RequestResult.Equals(RequestResult);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? base.GetHashCode();
        }
    }
}
