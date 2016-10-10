﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ValidationAlertsModel
	{
		private readonly IScheduleDictionary _schedules;
		private readonly NameOrderOption _nameOrderOption;
		private readonly DateOnlyPeriod _visiblePeriod;

		public ValidationAlertsModel(IScheduleDictionary schedules, NameOrderOption nameOrderOption, DateOnlyPeriod visiblePeriod)
		{
			_schedules = schedules;
			_nameOrderOption = nameOrderOption;
			_visiblePeriod = visiblePeriod;
		}

		public IList<ValidationAlert> GetAlerts(IEnumerable<IPerson> filteredPersons)
		{
			var result = new List<ValidationAlert>();
			foreach (var person in filteredPersons)
			{
				var range = _schedules[person];
				if (!range.BusinessRuleResponseInternalCollection.Any())
					continue;
				foreach (var businessRuleResponse in range.BusinessRuleResponseInternalCollection)
				{
					if(!_visiblePeriod.Contains(businessRuleResponse.DateOnlyPeriod.StartDate))
						continue;

					result.Add(new ValidationAlert()
					{
						Date = businessRuleResponse.DateOnlyPeriod.StartDate,
						PersonName = businessRuleResponse.Person.Name.ToString(_nameOrderOption),
						Person = businessRuleResponse.Person,
						Alert = businessRuleResponse.Message,
						TypeName = businessRuleResponse.FriendlyName
					});
				}
			}

			return result;
		}

		public class ValidationAlert
		{
			public DateOnly Date { get; set; }
			public string PersonName { get; set; }
			public IPerson Person { get; set; }
			public string Alert { get; set; }
			public string TypeName { get; set; }
		}
	}
	
}