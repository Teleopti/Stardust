using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface IGroupingsCreator
    {
        IList<IGroupPage> CreateBuiltInGroupPages(bool includeNewHierarchyGrouping);
    }
}