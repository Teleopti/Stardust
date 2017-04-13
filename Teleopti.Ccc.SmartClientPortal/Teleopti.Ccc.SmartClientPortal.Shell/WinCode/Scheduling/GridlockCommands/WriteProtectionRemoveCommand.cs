using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class WriteProtectionRemoveCommand : IExecutableCommand
    {
        private readonly IList<IScheduleDay> _scheduleDays;
        private readonly ICollection<IPersonWriteProtectionInfo> _modifiedWriteProtections;

        public WriteProtectionRemoveCommand(IList<IScheduleDay> scheduleDays, ICollection<IPersonWriteProtectionInfo> modifiedWriteProtections)
        {
            _scheduleDays = scheduleDays;
            _modifiedWriteProtections = modifiedWriteProtections;
        }

        public void Execute()
        {
            foreach (var scheduleDay in _scheduleDays)
            {
                var person = scheduleDay.Person;
                var protectedDate = person.PersonWriteProtection.PersonWriteProtectedDate;

                if (!protectedDate.HasValue || protectedDate.Value < scheduleDay.DateOnlyAsPeriod.DateOnly) continue;
                person.PersonWriteProtection.PersonWriteProtectedDate = null;
                _modifiedWriteProtections.Add(person.PersonWriteProtection);
            }
        }
    }
}
