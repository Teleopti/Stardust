using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
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
			var result = new HashSet<ValidationAlert>();
			foreach (var person in filteredPersons)
			{
				var range = _schedules[person];
				if (!range.BusinessRuleResponseInternalCollection.Any())
					continue;
				foreach (var businessRuleResponse in range.BusinessRuleResponseInternalCollection)
				{
					if(!_visiblePeriod.Contains(businessRuleResponse.DateOnlyPeriod.StartDate))
						continue;

					var alert = new ValidationAlert(businessRuleResponse.DateOnlyPeriod.StartDate,
						businessRuleResponse.Person.Name.ToString(_nameOrderOption), businessRuleResponse.FriendlyName);
					alert.Person = businessRuleResponse.Person;
					alert.Alert = businessRuleResponse.Message;
					result.Add(alert);
				}
			}

			return result.ToList();
		}

		public class ValidationAlert
		{
			public ValidationAlert(DateOnly date, string personName, string typeName)
			{
				Date = date;
				PersonName = personName;
				TypeName = typeName;
			}

			public DateOnly Date { get; }
			public string PersonName { get; }
			public IPerson Person { get; set; }
			public string Alert { get; set; }
			public string TypeName { get; }

			public override int GetHashCode()
			{
				return Date.GetHashCode() ^ PersonName.GetHashCode() ^ TypeName.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				return GetHashCode().Equals(obj.GetHashCode());
			}
		}
	}
	
}