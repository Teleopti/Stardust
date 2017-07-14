using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, RequestsTest]
	public class OvertimeRequestPersisterTest : ISetup
	{
		public IOvertimeRequestPersister Target;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeScheduleDictionary ScheduleDictionary;
		public FakeLoggedOnUser LoggedOnUser;

		private IPerson _person;
		private ICommandDispatcher _commandDispatcher;
		private DateTime currentDateTime = new DateTime();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePerson();
			_commandDispatcher = new FakeCommandDispatcher();
			
			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			system.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();
			system.UseTestDouble(new OvertimeRequestProcessor(_commandDispatcher, new ThisIsNow(currentDateTime), LoggedOnUser)).For<IOvertimeRequestProcessor>();
		}

		[Test]
		public void ShouldChangeStatusToPendingWhenPersistOvertimeRequest()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			MultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			var overtimeRequestForm = new OvertimeRequestForm
			{
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(currentDateTime),
					StartTime = new TimeOfDay(currentDateTime.AddMinutes(16).TimeOfDay), 
					EndDate = new DateOnly(currentDateTime),
					EndTime = new TimeOfDay(currentDateTime.AddHours(1).TimeOfDay)
				},
				MultiplicatorDefinitionSet = multiplicatorDefinitionSet.Id.Value
			};

			var result = Target.Persist(overtimeRequestForm);
			Assert.AreEqual(result.IsPending, true);
		}
	}
}