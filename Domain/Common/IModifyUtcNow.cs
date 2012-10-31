using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IModifyUtcNow
	{
		void SetUtcNow(DateTime? dateTime);
	}
}