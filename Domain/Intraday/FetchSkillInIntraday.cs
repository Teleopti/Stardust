using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class FetchSkillInIntraday
	{
		private readonly ILoadAllSkillInIntradays _loadAllSkillInIntradays;

		public FetchSkillInIntraday(ILoadAllSkillInIntradays loadAllSkillInIntradays)
		{
			_loadAllSkillInIntradays = loadAllSkillInIntradays;
		}

		public IEnumerable<SkillInIntraday> GetAll()
		{
			return _loadAllSkillInIntradays.Skills();
		}

		//public IEnumerable<SkillInIntraday> GetAllInOneSkillArea(Guid skillAreaId)
		//{
			
		//}
	}
}