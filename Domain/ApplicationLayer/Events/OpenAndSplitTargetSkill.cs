using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{

    public class OpenAndSplitTargetSkill : EventWithInfrastructureContext
    {

        public Guid JobId { get; set; }

        public Guid JobResultId { get; set; }

        public  Guid Identity
        {
            get { return JobId; }
        }

        public DateTime Date { get; set; }

        public Guid OwnerPersonId { get; set; }

        public Guid TargetSkillId { get; set; }

        public ICollection<IForecastsRow> Forecasts { get; set; }

        public TimeSpan StartOpenHour { get; set; }  
        
        public TimeSpan EndOpenHour { get; set; }
        
        public ImportForecastsMode ImportMode { get; set; }
    }
}
