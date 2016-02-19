using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{

    public class ScheduleOnNode : EventWithInfrastructureContext
    {

        public Guid JobId { get; set; }

        public  Guid Identity
        {
            get { return JobId; }
        }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid ScheduleIsland { get; set; }

    }


}