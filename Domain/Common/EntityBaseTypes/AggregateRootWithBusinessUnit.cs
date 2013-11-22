using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
    public abstract class VersionedAggregateRootWithBusinessUnit : VersionedAggregateRoot, 
                                                          IBelongsToBusinessUnit
    {
        private IBusinessUnit _businessUnit;

        public virtual IBusinessUnit BusinessUnit
        {
            get
            {
                if (_businessUnit == null)
                    _businessUnit = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
                return _businessUnit;
            }
			protected set { _businessUnit = value; }
        }
    }

	public abstract class NonversionedAggregateRootWithBusinessUnit : AggregateRoot,
														  IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit); }
			protected set { _businessUnit = value; }
		}
	}
}