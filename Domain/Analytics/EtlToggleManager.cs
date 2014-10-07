using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class EtlToggleManager : IEtlToggleManager
	{
		private readonly IList<EtlToggle> _toggles = new List<EtlToggle>();

		public void AddToggle(string key, bool isEnabled)
		{
			if(_toggles.SingleOrDefault(t => t.Key.Equals(key)) == null)
				_toggles.Add(new EtlToggle{Key = key, IsEnabled = isEnabled});
		}

		public bool IsEnabled(string toggle)
		{
			if(_toggles.SingleOrDefault(t => t.Key.Equals(toggle)) == null)
				return false;
			return _toggles.Single(t => t.Key.Equals(toggle)).IsEnabled;
		}
	}
}