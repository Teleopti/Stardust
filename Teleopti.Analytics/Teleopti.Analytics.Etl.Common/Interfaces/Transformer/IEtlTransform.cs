using System.Collections.Generic;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IEtlTransformer<T>
    {
        void Transform(IEnumerable<T> rootList, DataTable table);
    }
}