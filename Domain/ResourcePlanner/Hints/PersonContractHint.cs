﻿using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractHint : ISchedulePreHint
	{
		public void FillResult(HintResult hintResult, ScheduleHintInput input)
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
						ErrorResource = nameof(Resources.DeletedContractAssigned), 
						ErrorResourceData = new object[] { period.PersonContract.Contract.Description.Name }.ToList()
					}, GetType());
				}
			}
		}
	}
}