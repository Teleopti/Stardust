using System;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ScheduledAgentsForShiftTrade : IDataSetup
	{
		public DateTime Date { get; set; }
		public int PossibleTradeCount { get; set; }
		public string WorkflowControlSet { get; set; }
		public DateTime PersonPeriodStartDate { get; set; }
		public string ShiftCategory { get; set; }
		public string Team { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			for (int i = 0; i < PossibleTradeCount; i++)
			{
				string agentName = i.ToString(CultureInfo.InvariantCulture);

				var personPeriod = new PersonPeriodConfigurable { StartDate = PersonPeriodStartDate, Team = Team };
				DataMaker.Person(agentName).Apply(personPeriod);

				var userWorkflowControlSet = new WorkflowControlSetForUser { Name = WorkflowControlSet };
				DataMaker.Person(agentName).Apply(userWorkflowControlSet);

				DataMaker.Person(agentName).Apply(new ShiftConfigurable
				{
					ShiftCategory = ShiftCategory,
					StartTime = Date.AddHours(8),
					EndTime = Date.AddHours(16),
					LunchStartTime = Date.AddHours(12),
					LunchEndTime = Date.AddHours(13),
				});
			}
		}
	}
}
