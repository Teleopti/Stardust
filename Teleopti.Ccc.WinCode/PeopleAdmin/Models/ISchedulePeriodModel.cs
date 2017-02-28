using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Schedule period grid model interface.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-10-24
    /// </remarks>
   public interface ISchedulePeriodModel
    {
        ISchedulePeriod SchedulePeriod { get; }
    }
}
