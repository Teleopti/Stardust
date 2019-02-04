using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events_ChangeInfo_Versioned : 
		AggregateRoot_Events_ChangeInfo,
		IVersioned
	{
		private int? _version;

		public virtual int? Version => _version;

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}
}