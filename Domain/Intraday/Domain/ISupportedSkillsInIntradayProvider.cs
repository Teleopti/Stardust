﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface ISupportedSkillsInIntradayProvider
	{
		IList<ISkill> GetSupportedSkills(Guid[] skillIdList);
		bool CheckSupportedSkill(ISkill skill);
	}
}