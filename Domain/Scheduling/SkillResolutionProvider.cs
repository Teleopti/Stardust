using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISkillResolutionProvider
	{
		int MinimumResolution(IList<ISkill> skills);
	}

	public class SkillResolutionProvider : ISkillResolutionProvider
	{
		public int MinimumResolution(IList<ISkill> skills)
		{
			var minimumResolution = int.MaxValue;
		    if (skills != null)
		        foreach (var skill in skills)
		        {
		            var resolution = skill.DefaultResolution;
		            if (resolution < minimumResolution)
		                minimumResolution = resolution;
		        }
		    return minimumResolution;
		}
	}
}