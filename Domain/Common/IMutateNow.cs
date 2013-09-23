using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IMutateNow
	{
		void SetNow(DateTime? dateTime);
		bool IsExplicitlySet();
	}
}