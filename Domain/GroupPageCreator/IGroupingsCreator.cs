using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface IGroupingsCreator
    {
        IList<IGroupPage> CreateBuiltInGroupPages(bool includeNewHierarchyGrouping);
    }
}