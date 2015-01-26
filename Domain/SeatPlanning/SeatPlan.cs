using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.SeatPlanning
{

	[Serializable]
    public class SeatPlan : VersionedAggregateRoot, ISeatPlan, IDeleteTag
	{
		public DateOnlyPeriod Period
		{
			get { return _period; }
			set { _period = value; }
		}

		public SeatPlan(IScenario scenario)
		{
			_scenario = scenario;
		}

		public void CreateSeatPlan(DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			_period = period;

			//RobTodo: Fill in save routines when needed.
			addSeatPlanAddedEvent (period, trackedCommandInfo);

		}

		private void addSeatPlanAddedEvent(DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			
			var seatPlanAddedEvent = new SeatPlanAddedEvent
			{
				StartDate = period.StartDate,
				EndDate = period.EndDate,
				ScenarioId = _scenario.Id.GetValueOrDefault(),
				BusinessUnitId = _scenario.BusinessUnit.Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				seatPlanAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				seatPlanAddedEvent.TrackId = trackedCommandInfo.TrackId;
			}
			AddEvent(seatPlanAddedEvent);
		}
		
		#region IDeleteTag Implementation
		
		private bool _isDeleted;
		private IScenario _scenario;
		private DateOnlyPeriod _period;

		public bool IsDeleted
		{
			get { return _isDeleted; }
			private set { _isDeleted = value; }
		}

		public void SetDeleted()
		{
			_isDeleted = true;
		}
		
		#endregion



	}


}
