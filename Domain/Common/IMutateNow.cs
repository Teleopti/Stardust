using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IMutateNow
	{
		void Mutate(DateTime? utc);
		bool IsMutated();
	}
}