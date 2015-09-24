using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IMutateNow
	{
		void Reset();
		void Is(DateTime? utc);
		bool IsMutated();
	}
}