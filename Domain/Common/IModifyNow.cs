using System;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IModifyNow
	{
		void SetNow(DateTime? dateTime);
	}
}