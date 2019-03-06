using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit : 
		AggregateRoot_Events_ChangeInfo_Versioned,
		IFilterOnBusinessUnit
	{
		protected IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit; }
			set { _businessUnit = value; }
		}
		
		public virtual IBusinessUnit GetOrFillWithBusinessUnit_DONTUSE() => _businessUnit ?? (_businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
	}
}