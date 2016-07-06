using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class ScheduleValidationProvider:IScheduleValidationProvider
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;

		public ScheduleValidationProvider(IScheduleStorage scheduleStorage,ICurrentScenario currentScenario,IPersonRepository personRepository)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
		}


		public IList<BusinessRuleValidationResult> GetBusineeRuleValidationResults(FetchRuleValidationResultFormData input)
		{
			throw new NotImplementedException();
		}
	}

	public interface IScheduleValidationProvider
	{
		IList<BusinessRuleValidationResult> GetBusineeRuleValidationResults(FetchRuleValidationResultFormData input);
	}
}