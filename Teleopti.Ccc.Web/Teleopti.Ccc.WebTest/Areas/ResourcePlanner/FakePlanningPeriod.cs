using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FakePlanningPeriod : IPlanningPeriod
	{
		public FakePlanningPeriod(Guid id, DateOnlyPeriod range)
		{
			Id = id;
			Range = range;
		}
		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}

		public Guid? Id { get; private set; }
		public void SetId(Guid? newId)
		{
			Id = newId.Value;
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}

		public DateOnlyPeriod Range { get; private set; }
		public void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation)
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod NextPlanningPeriod()
		{
			throw new NotImplementedException();
		}

		public PlanningPeriodState State { get; private set; }
		public void Scheduled()
		{
			throw new NotImplementedException();
		}

		public void Publish(params IPerson[] people)
		{
			throw new NotImplementedException();
		}
	}
}