using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class HintResult
	{
		public ICollection<SchedulingHintError> InvalidResources { get; set; }

		public HintResult()
		{
			InvalidResources = new List<SchedulingHintError>();
		}

		public void Add(PersonHintError error, Type validationType)
		{
			if (InvalidResources.Any(x => x.ResourceId == error.PersonId))
			{
				var schedulingValidationError = InvalidResources.First(x => x.ResourceId == error.PersonId);
				schedulingValidationError.ValidationErrors.Add(error.ValidationError);
				schedulingValidationError.ValidationTypes.Add(validationType);
			}
			else
				InvalidResources.Add(new SchedulingHintError
				{
					ResourceId = error.PersonId,
					ResourceName = error.PersonName,
					ValidationErrors = new List<string> {error.ValidationError},
					ResourceType = ValidationResourceType.Agent,
					ValidationTypes = new List<Type> { validationType}
				});
		}

		public void Add(MissingForecastModel error, Type validationType)
		{
			if (InvalidResources.Any(x => x.ResourceName == error.SkillName))
			{
				foreach (var err in error.MissingRanges.Select(x => $"{Resources.MissingForecastFrom} {x.StartDate:d} {Resources.ToText} {x.EndDate:d}"))
				{
					var schedulingValidationError = InvalidResources.First(x => x.ResourceName == error.SkillName);
					schedulingValidationError.ValidationErrors.Add(err);
					schedulingValidationError.ValidationTypes.Add(validationType);
				}
			}
			else
			{
				InvalidResources.Add(new SchedulingHintError
				{
					ResourceName = error.SkillName,
					ValidationErrors = error.MissingRanges.Select(x => $"{Resources.MissingForecastFrom} {x.StartDate:d} {Resources.ToText} {x.EndDate:d}").ToList(),
					ResourceType = ValidationResourceType.Skill,
					ResourceId = error.SkillId,
					ValidationTypes = new List<Type> { validationType }
				});
			}
		}
	}
}