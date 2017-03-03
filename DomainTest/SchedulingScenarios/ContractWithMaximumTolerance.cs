using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class ContractWithMaximumTolerance : Contract
	{
		public ContractWithMaximumTolerance()
		{
			Description = new Description("contract with max tolerance");
		}

		public override TimeSpan NegativePeriodWorkTimeTolerance
		{
			get { return TimeSpan.FromDays(20); }
			set { base.NegativePeriodWorkTimeTolerance = value; }
		}

		public override TimeSpan PositivePeriodWorkTimeTolerance
		{
			get { return TimeSpan.FromDays(20); }
			set { base.NegativePeriodWorkTimeTolerance = value; }
		}
	}
}