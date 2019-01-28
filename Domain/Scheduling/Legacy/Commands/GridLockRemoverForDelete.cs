using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    public interface IGridlockRemoverForDelete
    {
        IList<IScheduleDay> RemoveLocked(IList<IScheduleDay> source);
    }

    public class GridlockRemoverForDelete : IGridlockRemoverForDelete
    {
        private readonly IGridlockManager _gridlockManager;

        public GridlockRemoverForDelete(IGridlockManager gridlockManager)
        {
            _gridlockManager = gridlockManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IList<IScheduleDay> RemoveLocked(IList<IScheduleDay> source)
        {
            IList<IScheduleDay> toRemove = new List<IScheduleDay>();

            foreach (var scheduleDay in source)
            {
                GridlockDictionary lockDictionary = _gridlockManager.Gridlocks(scheduleDay);
                if (lockDictionary != null && lockDictionary.Count != 0)
                {
                    // if it only is one lock and that is scheduleDay AND the user is allowed to change those
                    // Don't remove it the user can change it
                    var gridlock = new Gridlock(scheduleDay, LockType.WriteProtected);
                    if (lockDictionary.Count == 1 && lockDictionary.ContainsKey(gridlock.Key) &&
                        PrincipalAuthorization.Current_DONTUSE().IsPermitted(
                            DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
                    {

                    }
                    else
                    {
                        gridlock = new Gridlock(scheduleDay, LockType.OutsidePersonPeriod);
                        if (lockDictionary.Count == 1 && lockDictionary.ContainsKey(gridlock.Key))
                            continue;

                        toRemove.Add(scheduleDay);
                    }
                }
            }
            foreach (var scheduleDay in toRemove)
            {
                source.Remove(scheduleDay);
            }

            return source;
        }
    }
}