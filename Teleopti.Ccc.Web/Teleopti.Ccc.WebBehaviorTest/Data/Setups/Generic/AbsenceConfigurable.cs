using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public bool? InContractTime { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var absence = new Absence {Description = new Description(Name)};
			if (InContractTime.HasValue)
				absence.InContractTime = InContractTime.Value;
			var absenceRepository = new AbsenceRepository(uow);
			absenceRepository.Add(absence);
		}
	}
}