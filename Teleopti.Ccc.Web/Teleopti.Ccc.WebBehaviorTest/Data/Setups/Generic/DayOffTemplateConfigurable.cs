using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class DayOffTemplateConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var dayOffTemplate = new DayOffTemplate(new Description(Name));
			var dayOffRepository = new DayOffRepository(uow);
			dayOffRepository.Add(dayOffTemplate);
		}
	}
}