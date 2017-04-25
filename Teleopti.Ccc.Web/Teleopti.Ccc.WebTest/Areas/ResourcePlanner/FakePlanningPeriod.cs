using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FakePlanningPeriod : IPlanningPeriod
	{
		public FakePlanningPeriod(Guid id, DateOnlyPeriod range, IAgentGroup agentGroup)
		{
			Id = id;
			Range = range;
			AgentGroup = agentGroup;
		}

		public FakePlanningPeriod(Guid id, DateOnlyPeriod range)
		{
			Id = id;
			Range = range;
			AgentGroup = null;
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
		public void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, bool updateTypeAndNumber = false)
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod NextPlanningPeriod(IAgentGroup agentGroup)
		{
			throw new NotImplementedException();
		}

		public PlanningPeriodState State { get; private set; }
		public IAgentGroup AgentGroup { get; private set; }
		public ISet<IJobResult> JobResults { get; }

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