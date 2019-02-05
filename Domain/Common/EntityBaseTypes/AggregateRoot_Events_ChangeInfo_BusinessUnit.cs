using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events_ChangeInfo_BusinessUnit : 
		AggregateRoot_Events_ChangeInfo,
		IFilterOnBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			protected set => _businessUnit = value;
		}
	}
}