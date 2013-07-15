using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public interface IScheduleOvertimeCommand
    {
        void  Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
                                      IList<IScheduleDay> selectedSchedules);
    }

    public class ScheduleOvertimeCommand: IScheduleOvertimeCommand
    {
        private BackgroundWorker _backgroundWorker;

        public ScheduleOvertimeCommand()
        {
            
        }

        public void  Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
                       IList<IScheduleDay> selectedSchedules)
        {
            _backgroundWorker = backgroundWorker;
            foreach (var scheduleDay in selectedSchedules)
            {
                //Randomly select one of the selected agents that does not end his shift with overtime

                //Calculate best length (if any) for overtime

                //extend shift
            }
        }
    }

    
}
