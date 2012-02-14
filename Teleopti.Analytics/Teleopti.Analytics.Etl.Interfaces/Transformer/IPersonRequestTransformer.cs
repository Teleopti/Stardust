using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IPersonRequestTransformer<T>
    {
        void Transform(IEnumerable<T> rootList, int intervalsPerDay, DataTable table);
    }
}
