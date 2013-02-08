using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.Model;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
	public class PersonDetailContainer
	{
		public PersonDto Person { get; set; }
		public PersonPeriodDetailDto PersonPeriod { get; set; }
		public PersonSkillPeriodDto PersonSkillPeriod { get; set; }
		public string Contract { get; set; }
		public string ContractSchedule { get; set; }
		public string PartTimePercentageName { get; set; }
		public string PartTimePercentageValue { get; set; }

		public SiteModel Site { get; set; }
	}
}