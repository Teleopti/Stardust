using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class NonversionedAggregateRootWithBusinessUnit : AggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}
	}
}