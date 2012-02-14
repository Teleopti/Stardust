using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    [Serializable]
    public class OptimizeActivitiesSettings
    {
        public bool KeepShiftCategory { get; set; }
        public bool KeepStartTime { get; set; }
        public bool KeepEndTime { get; set; }
        public TimePeriod? AllowAlterBetween { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public IList<IActivity> DoNotMoveActivities { get; set; }
        public IList<Guid> DoNotMoveActivitiesGuids { get; set; }

        public OptimizeActivitiesSettings()
        {
            //DoNotMoveActivities = new List<IActivity>();
            DoNotMoveActivitiesGuids = new List<Guid>();
        }  
    }
}