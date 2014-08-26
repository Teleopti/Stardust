using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class RequestableAbsenceType : IUserDataSetup
	{
		private readonly string _name;
		private readonly string _tracker;

		public RequestableAbsenceType(string name)
		{
			_name = name;
		}

		public RequestableAbsenceType(RequestableAbsenceFields requestableAbsenceFields)
		{
			_name = requestableAbsenceFields.Name;
			_tracker = requestableAbsenceFields.TrackerType;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			ITracker tracker = null;
			if (!string.IsNullOrEmpty(_tracker))
			{
				switch (_tracker.ToLower())
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
			}

			new AbsenceRepository(uow).Add(new Absence
			                               	{
			                               		Description = new Description(_name),
			                               		Requestable = true,
												Tracker = tracker,
												InContractTime  = true
			                               	});
		}
	}

	public class RequestableAbsenceFields
	{
		public string Name { get; set; }
		public string TrackerType { get; set; }
	}
}