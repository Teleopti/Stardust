using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AbsenceConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
		public bool? InContractTime { get; set; }
		public bool? Confidential { get; set; }
		public bool? Requestable { get; set; }

		public Absence Absence;

		public void Apply(IUnitOfWork uow)
		{
			Absence = new Absence {Description = new Description(Name, ShortName)};

			if (Color != null)
				Absence.DisplayColor = System.Drawing.Color.FromName(Color);

			if (InContractTime.HasValue)
				Absence.InContractTime = InContractTime.Value;

			if (Confidential.HasValue)
				Absence.Confidential = Confidential.Value;

			if (Requestable.HasValue)
				Absence.Requestable = Requestable.Value;

			var absenceRepository = new AbsenceRepository(uow);
			absenceRepository.Add(Absence);
		}
	}
}