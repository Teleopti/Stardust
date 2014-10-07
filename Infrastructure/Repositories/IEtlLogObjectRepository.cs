using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlLogObjectRepository
	{
		IEnumerable<LogObjectDetail> Load();
	}
}