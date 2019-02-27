using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

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
		public string TrackerType { get; set; }

		public Absence Absence;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			Absence = new Absence { Description = new Description(Name, ShortName) };

			ITracker tracker = null;
			if (!string.IsNullOrEmpty(TrackerType))
			{
				switch (TrackerType.ToLower())
				{
					case "day":
						tracker = Tracker.CreateDayTracker();
						break;
					case "time":
						tracker = Tracker.CreateTimeTracker();
						break;
					case "overtime":
						tracker = Tracker.CreateOvertimeTracker();
						break;
					case "comp":
						tracker = Tracker.CreateCompTracker();
						break;
				}
				Absence.Tracker = tracker;
			}

			

			if (Color != null)
				Absence.DisplayColor = System.Drawing.Color.FromName(Color);

			if (InContractTime.HasValue)
				Absence.InContractTime = InContractTime.Value;

			if (Confidential.HasValue)
				Absence.Confidential = Confidential.Value;

			if (Requestable.HasValue)
				Absence.Requestable = Requestable.Value;
			
			var absenceRepository = AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork);
			absenceRepository.Add(Absence);
		}
	}
}