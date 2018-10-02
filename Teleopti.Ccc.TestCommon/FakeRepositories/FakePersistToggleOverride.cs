using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersistToggleOverride : IPersistToggleOverride
	{
		public void Save(Toggles toggle, bool value)
		{
			throw new System.NotImplementedException();
		}

		public IList<string> DeletedToggles { get; } = new List<string>();
		public void Delete(string toggle)
		{
			DeletedToggles.Add(toggle);
		}
	}
}