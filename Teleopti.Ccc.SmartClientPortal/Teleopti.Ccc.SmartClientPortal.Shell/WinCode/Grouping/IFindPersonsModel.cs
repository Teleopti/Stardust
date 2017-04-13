using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Grouping
{
    public interface IFindPersonsModel
    {
        DateOnly FromDate { get; set; }
        DateOnly ToDate { get; set; }
        IEnumerable<IPerson> Persons { get; }
    }
}
