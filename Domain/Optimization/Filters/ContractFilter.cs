using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class ContractFilter : Entity, IFilter
	{
		protected ContractFilter()
		{
		}

		public ContractFilter(IContract contract)
		{
			Contract = contract;
		}

		public virtual IContract Contract { get; protected set; }

		public virtual bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && person.Period(dateOnly).PersonContract.Contract.Equals(Contract);
		}

		public virtual string FilterType
		{
			get { return "contract"; }
		}

		public override bool Equals(IEntity other)
		{
			var otherContractFilter = other as ContractFilter;
			if (otherContractFilter == null)
				return false;

			return Contract.Equals(otherContractFilter.Contract);
		}

		public override int GetHashCode()
		{
			return Contract.GetHashCode();
		}
	}
}