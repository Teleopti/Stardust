using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceTimeProviderCache
	{
		void Setup();
		IEnumerable<PayloadWorkTime> Get(string key);
		void Add(string key, IEnumerable<PayloadWorkTime> absenceTime);
		string GetConfigValue();
	}
}