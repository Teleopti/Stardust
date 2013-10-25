using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class RequestableAbsenceType : IUserDataSetup
	{
		private readonly string _name;

		public RequestableAbsenceType(string name)
		{
			_name = name;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			new AbsenceRepository(uow).Add(new Absence
			                               	{
			                               		Description = new Description(_name),
			                               		Requestable = true
			                               	});
		}
	}
}