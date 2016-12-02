using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface ISkillGroupContext
	{
		IDisposable Create(DateOnlyPeriod period);
	}
}