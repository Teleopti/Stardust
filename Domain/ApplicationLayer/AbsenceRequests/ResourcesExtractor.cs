using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourcesExtractor : IResourcesForShovelAndCalculation
	{
		private readonly IEnumerable<SkillCombinationResource> _resources;
		private readonly IEnumerable<ISkill> _skills;

		public ResourcesExtractor(IEnumerable<SkillCombinationResource> resources, IEnumerable<ISkill> skills, int resolution)
		{
			_resources = resources;
			_skills = skills;
			MinSkillResolution = resolution;
		}

		public IDictionary<string, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var skillDictionary = _skills.Where(s => s.Activity!=null && s.Activity.Equals(activity)).ToLookup(s => s.Id.GetValueOrDefault());
			var withSkills =
				_resources.Select(
					r =>
						new
						{
							Resource = r,
							Skills = r.SkillCombination.Select(s => skillDictionary[s].FirstOrDefault()).Where(s => s != null)
						}).Where(s => s.Resource.StartDateTime==periodToCalculate.StartDateTime && s.Skills.Any());

			return withSkills.ToDictionary(k => string.Join("_", k.Resource.SkillCombination),
				v =>
					new AffectedSkills
					{
						Count = double.NaN,
						Resource = v.Resource.Resource,
						SkillEffiencies = new ConcurrentDictionary<Guid, double>(),
						Skills = v.Skills
					});
		}

		public int MinSkillResolution { get; }
	}
}