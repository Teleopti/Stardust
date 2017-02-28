using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExtendedPreferenceTemplateRepository : IRepository<IExtendedPreferenceTemplate>
	{
		IList<IExtendedPreferenceTemplate> FindByUser(IPerson user);
		IExtendedPreferenceTemplate Find(Guid id);
	}
}
