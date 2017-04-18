using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IPeopleLoader 
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-08
        /// </remarks>
        ISchedulerStateHolder Initialize();
    }
}