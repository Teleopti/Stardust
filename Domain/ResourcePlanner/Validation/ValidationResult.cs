using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class ValidationResult
	{
		public ICollection<SchedulingValidationError> InvalidResources { get; set; }

		public ValidationResult()
		{
			InvalidResources = new List<SchedulingValidationError>();
		}

		public void Add(PersonValidationError error)
		{
			if (InvalidResources.Any(x => x.ResourceId == error.PersonId))
				InvalidResources.First(x => x.ResourceId == error.PersonId).ValidationErrors.Add(error.ValidationError);
			else
				InvalidResources.Add(new SchedulingValidationError
				{
					ResourceId = error.PersonId,
					ResourceName = error.PersonName,
					ValidationErrors = new List<string> {error.ValidationError},
					ResourceType = ValidationResourceType.Agent
				});
		}

		public void Add(MissingForecastModel error)
		{
			if (InvalidResources.Any(x => x.ResourceName == error.SkillName))
			{
				foreach (var err in error.MissingRanges.Select(x => $"{Resources.MissingForecastFrom} {x.StartDate:d} {Resources.ToText} {x.EndDate:d}"))
				{
					InvalidResources.First(x => x.ResourceName == error.SkillName)
						.ValidationErrors.Add(err);
				}
			}
			else
			{
				InvalidResources.Add(new SchedulingValidationError
				{
					ResourceName = error.SkillName,
					ValidationErrors = error.MissingRanges.Select(x => $"{Resources.MissingForecastFrom} {x.StartDate:d} {Resources.ToText} {x.EndDate:d}").ToList(),
					ResourceType = ValidationResourceType.Skill,
					ResourceId = error.SkillId
				});
			}
		}
	}
}