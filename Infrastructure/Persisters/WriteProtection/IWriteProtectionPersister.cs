using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.WriteProtection
{
	public interface IWriteProtectionPersister
	{
		void Persist(ICollection<IPersonWriteProtectionInfo> writeProtections);
	}
}