using System.Collections.Generic;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IPersonRequestTransformer<T>
    {
        void Transform(IEnumerable<T> rootList, int intervalsPerDay, DataTable table);
    }
}
