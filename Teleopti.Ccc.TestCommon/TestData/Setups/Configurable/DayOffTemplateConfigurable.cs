using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class DayOffTemplateConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string ShortName { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var dayOffTemplate = new DayOffTemplate(new Description(Name, ShortName));

			var dayOffRepository = new DayOffTemplateRepository(currentUnitOfWork);
			dayOffRepository.Add(dayOffTemplate);
		}
	}
}