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

		public void Add(PersonHintError error, Type validationType, ValidationResourceType category = ValidationResourceType.Basic)
		{
			if (InvalidResources.Any(x => x.ResourceId == error.PersonId))
			{
				var schedulingValidationError = InvalidResources.First(x => x.ResourceId == error.PersonId);
				//schedulingValidationError.ValidationErrors.Add(error.ValidationError);
				schedulingValidationError.ValidationErrors.Add(new ValidationError(){ErrorMessage = error.ValidationError,ResourceType = category});
				schedulingValidationError.ValidationTypes.Add(validationType);
				//schedulingValidationError.ResourceTypes.Add(category);
			}
			else
				InvalidResources.Add(new SchedulingHintError
				{
					ResourceId = error.PersonId,
					ResourceName = error.PersonName,
					ValidationErrors = new List<ValidationError> { new ValidationError() { ErrorMessage = error.ValidationError, ResourceType = category } },
					//ResourceTypes = new List<ValidationResourceType> { category },
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
					schedulingValidationError.ValidationErrors.Add(new ValidationError() { ErrorMessage = err, ResourceType = ValidationResourceType.Skill });
					schedulingValidationError.ValidationTypes.Add(validationType);
				}
			}
			else
			{
				var validationErrors = new List<ValidationError>();
				foreach (var err in error.MissingRanges.Select(x => $"{Resources.MissingForecastFrom} {x.StartDate:d} {Resources.ToText} {x.EndDate:d}"))
				{
					validationErrors.Add(new ValidationError()
					{
						ErrorMessage = err, ResourceType = ValidationResourceType.Skill
					});
				}
				InvalidResources.Add(new SchedulingHintError
				{
					ResourceName = error.SkillName,
					ValidationErrors = validationErrors,
					//ResourceTypes = new List<ValidationResourceType>{ ValidationResourceType.Skill },
					ResourceId = error.SkillId,
					ValidationTypes = new List<Type> {validationType}
				});
			}
		}
	}
}