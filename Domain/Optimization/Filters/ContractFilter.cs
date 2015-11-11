using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class ContractFilter : IFilter
	{
		private readonly IContract _contract;

		public ContractFilter(IContract contract)
		{
			_contract = contract;
		}

		public bool IsValidFor(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			return personPeriod != null && person.Period(dateOnly).PersonContract.Contract.Equals(_contract);
		}

		public string FilterType
		{
			get { return "contract"; }
		}
	}
}