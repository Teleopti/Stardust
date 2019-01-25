using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class VersionedAggregateRootWithBusinessUnit : VersionedAggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}
	}
}