using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOutboundCampaign : IAggregateRoot, IDeleteTag, ICloneableEntity<IOutboundCampaign>
    {
        string Name { get; set; }

        ISkill Skill { get; set; }        

        int CallListLen { get; set; }        

        int TargetRate { get; set; }       

        int ConnectRate { get; set; }        

        int RightPartyConnectRate { get; set; }        

        int ConnectAverageHandlingTime { get; set; }        

        int RightPartyAverageHandlingTime { get; set; }
       
        int UnproductiveTime { get; set; }        

        IDictionary<DayOfWeek, TimePeriod> WorkingHours { get; set; }

		  DateTimePeriod SpanningPeriod { get; set; }
		  DateOnlyPeriod BelongsToPeriod { get; set; }

        int CampaignTasks();

        TimeSpan AverageTaskHandlingTime();

        TimeSpan? GetManualProductionPlan(DateOnly date);

        void SetManualProductionPlan(DateOnly date, TimeSpan time);

        void ClearProductionPlan(DateOnly date);

        void SetActualBacklog(DateOnly date, TimeSpan time);

        void ClearActualBacklog(DateOnly date);

        TimeSpan? GetActualBacklog(DateOnly date);

    }
}
