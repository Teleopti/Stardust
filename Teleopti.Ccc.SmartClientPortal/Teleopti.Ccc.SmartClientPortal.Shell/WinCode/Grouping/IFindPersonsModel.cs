using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping
{
    public interface IFindPersonsModel
    {
        DateOnly FromDate { get; set; }
        DateOnly ToDate { get; set; }
        IEnumerable<IPerson> Persons { get; }
    }
}
