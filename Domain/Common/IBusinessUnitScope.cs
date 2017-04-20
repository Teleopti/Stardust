using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IBusinessUnitScope
	{
		IDisposable OnThisThreadUse(IBusinessUnit businessUnit);
	}
}