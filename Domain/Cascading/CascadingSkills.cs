using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingSkills : IEnumerable<ISkill>
	{
		private readonly IEnumerable<ISkill> _cascadingSkills;

		public CascadingSkills(IEnumerable<ISkill> allSkills)
		{
			_cascadingSkills = allSkills.Where(x => x.IsCascading()).OrderBy(x => x.CascadingIndex).ToArray();
		}

		public IEnumerable<ISkill> ForActivity(IActivity activity)
		{
			return _cascadingSkills.Where(x => x.Activity.Equals(activity));
		}

		public IEnumerable<ActivityAndIntervalLength> AffectedActivities()
		{
			return _cascadingSkills.Select(x => x.Activity).Distinct().Select(activity => 
				new ActivityAndIntervalLength(activity, TimeSpan.FromMinutes(_cascadingSkills.First(x => x.Activity.Equals(activity)).DefaultResolution)));
		}

		public IEnumerator<ISkill> GetEnumerator()
		{
			return _cascadingSkills.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}