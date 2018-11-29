using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingOvertimeRequest : IUserDataSetup
	{
		public IPersonRequest PersonRequest { get; set; }

		public string Subject { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow;

			var multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(unitOfWork);
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime);
			multiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			var overtimeRequest = new OvertimeRequest(multiplicatorDefinitionSet, new DateTimePeriod(today.AddHours(10), today.AddHours(11)));
			PersonRequest = new PersonRequest(person, overtimeRequest) {Subject = Subject };
			PersonRequest.TrySetMessage(
				"This is just a short text that doesn't say anything, except explaining that it doesn't say anything");

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}
}