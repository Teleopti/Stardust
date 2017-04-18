using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
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
