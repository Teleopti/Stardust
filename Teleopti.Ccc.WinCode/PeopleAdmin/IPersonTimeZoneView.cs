using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	public interface IPersonTimeZoneView
	{
		void Cancel();
		void SetPersonsTimeZone(IList<IPerson> persons, TimeZoneInfo timeZoneInfo);
	}
}
