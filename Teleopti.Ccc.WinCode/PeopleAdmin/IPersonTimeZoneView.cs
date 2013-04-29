using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	public interface IPersonTimeZoneView
	{
		void Cancel();
		void SetPersonTimeZone(IPerson person);
		void HideDialog();
	}
}
