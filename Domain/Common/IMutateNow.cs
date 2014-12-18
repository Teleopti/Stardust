using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IMutateNow
	{
		void Is(DateTime? utc);
		bool IsMutated();
	}
}