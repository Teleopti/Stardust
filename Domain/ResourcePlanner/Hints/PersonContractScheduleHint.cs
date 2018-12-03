using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractScheduleHint : ISchedulePreHint
	{
		public void FillResult(HintResult hintResult, ScheduleHintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (periods.Any(x => x.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)) continue;
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.ContractSchedule).IsDeleted))
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.DeletedContractScheduleAssigned),
						ErrorResourceData = new object[] { period.PersonContract.ContractSchedule.Description.Name }.ToList()
					}, GetType());
				}
			}
		}
	}
}