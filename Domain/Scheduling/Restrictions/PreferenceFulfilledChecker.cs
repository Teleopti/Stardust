using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{

	/// <summary>
	/// Checks if the preference is fullfilled for a given day
	/// </summary>
	public interface IPreferenceFulfilledChecker
	{
		bool IsPreferenceFulfilled(DateOnly theDay);
	}


	public class PreferenceFulfilledChecker : IPreferenceFulfilledChecker
	{
		public bool IsPreferenceFulfilled(DateOnly theDay)
		{
			return false;
		}
	}
}
