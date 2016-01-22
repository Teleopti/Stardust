using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class FakeToggleManager : IToggleManager, IScheduleCommandToggle
	{
		private readonly IList<Toggles> _enabled = new List<Toggles>();

		public FakeToggleManager(Toggles toggle)
		{
			Enable(toggle);
		}

		public FakeToggleManager()
		{
		}

		public void Enable(Toggles toggle)
		{
			_enabled.Add(toggle);
		}

		public void Disable(Toggles toggle)
		{
			_enabled.Remove(toggle);
		}

		public bool IsEnabled(Toggles toggle)
		{
			return _enabled.Any(e => e == toggle);
		}

		public void DisableAll()
		{
			_enabled.Clear();
		}

		public void EnableAll()
		{
			Enum.GetValues(typeof (Toggles)).Cast<Toggles>().ForEach(Enable);
		}
	}


}