using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class DayOffTemplateConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var dayOffTemplate = new DayOffTemplate(new Description(Name));
			var dayOffRepository = new DayOffTemplateRepository(currentUnitOfWork);
			dayOffRepository.Add(dayOffTemplate);
		}
	}
}