using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public interface IUserNow
	{
		DateTime DateTime();
		DateOnly Date();
	}
}