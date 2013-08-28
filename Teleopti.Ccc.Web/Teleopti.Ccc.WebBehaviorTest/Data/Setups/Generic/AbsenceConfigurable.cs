using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Color { get; set; }
		public bool? InContractTime { get; set; }
		public bool? Confidential { get; set; }

		public Absence Absence;

		public void Apply(IUnitOfWork uow)
		{
			Absence = new Absence {Description = new Description(Name)};

			if (Color != null)
				Absence.DisplayColor = System.Drawing.Color.FromName(Color);

			if (InContractTime.HasValue)
				Absence.InContractTime = InContractTime.Value;

			if (Confidential.HasValue)
				Absence.Confidential = Confidential.Value;
			var absenceRepository = new AbsenceRepository(uow);
			absenceRepository.Add(Absence);
		}
	}
}