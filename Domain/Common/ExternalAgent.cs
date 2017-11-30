using System;

namespace Teleopti.Ccc.Domain.Common
{
	public class ExternalAgent : Person
	{
		public override Guid? Id => Guid.NewGuid();
		public override bool IsExternalAgent => true;
	}
}