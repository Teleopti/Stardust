using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.Contract).IsDeleted))
				{
					hintResult.Add(new PersonHintError(person)
					{
						ValidationError = string.Format(Resources.DeletedContractAssigned, period.PersonContract.Contract.Description.Name)
					}, GetType());
				}
			}
		}
	}
}