using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class FakeAllToggles : IAllToggles
	{
		private readonly HashSet<Toggles> _toggles = new HashSet<Toggles>();

		public FakeAllToggles(params Toggles[] toggles)
		{
			toggles.ForEach(t => _toggles.Add(t));
		}
		public ISet<Toggles> Toggles()
		{
			return _toggles;
		}
	}
	public class FakeToggleManager : IToggleManager
	{
		private readonly IList<Toggles> _enabled = new List<Toggles>();

		public FakeToggleManager(Toggles toggle)
		{
			Enable(toggle);
		}

		public FakeToggleManager()
		{
		}

		public void Set(Toggles toggle, bool value)
		{
			if(value)
			{
				_enabled.Add(toggle);
			}
			else
			{
				_enabled.Remove(toggle);
			}
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