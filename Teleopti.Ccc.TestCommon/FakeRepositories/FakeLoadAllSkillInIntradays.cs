using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeLoadAllSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly IList<SkillInIntraday> _skillInIntradays = new List<SkillInIntraday>();

		public void Has(SkillInIntraday existingSkillInIntraday)
		{
			_skillInIntradays.Add(existingSkillInIntraday);
		}

		public IEnumerable<SkillInIntraday> Skills()
		{
			return _skillInIntradays;
		}

		public static void HasWithName(string name)
		{
			var skillInIntraday = new SkillInIntraday();
			//typeof(SkillInIntraday).Get
		}
	}
}