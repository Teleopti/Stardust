using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IGroupPagePersonTransformer
    {
        IEnumerable<IGroupPage> UserDefinedGroupings { get; }
        IList<IGroupPage> BuiltInGroupPages { get; }
        void Transform(IList<IGroupPage> builtInGroupPages, IEnumerable<IGroupPage> userDefinedPages, DataTable table);
    }
}
