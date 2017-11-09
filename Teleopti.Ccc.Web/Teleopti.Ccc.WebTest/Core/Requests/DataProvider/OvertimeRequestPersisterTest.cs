using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[RequestsTest]
	public class OvertimeRequestPersisterTest : ISetup
	{
		public IOvertimeRequestPersister Target;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public FakeScheduleDictionary ScheduleDictionary;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeToggleManager FakeToggleManager;

		private IPerson _person;
		private FakeCommandDispatcher _commandDispatcher;
		private DateTime currentDateTime = new DateTime(2017, 11, 07);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePerson();
			_commandDispatcher = new FakeCommandDispatcher();

			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
			system.UseTestDouble<FakeScheduleDictionary>().For<IScheduleDictionary>();

			system.UseTestDouble(new OvertimeRequestProcessor(_commandDispatcher, new IOvertimeRequestValidator[] {}, 
				new FakeActivityRepository(), new FakeSkillRepository(), new FakeSkillTypeRepository(), new FakeOvertimeRequestAvailableSkillsValidator()))
				.For<IOvertimeRequestProcessor>();
			system.UseTestDouble(new ThisIsNow(currentDateTime)).For<INow>();
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

		[Test]
		[SetCulture("en-US")]
		public void ShouldReProcessWhenRequestPeriodIsChanged()
		{
			FakeToggleManager.Enable(Toggles.OvertimeRequestPeriodSetting_46417);
			var now = new DateOnly(currentDateTime);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 2),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			MultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);
			var requestStartDateTime = currentDateTime.AddDays(3);
			var overtimeRequestForm = new OvertimeRequestForm
			{
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(requestStartDateTime),
					StartTime = new TimeOfDay(requestStartDateTime.TimeOfDay),
					EndDate = new DateOnly(requestStartDateTime),
					EndTime = new TimeOfDay(requestStartDateTime.AddHours(1).TimeOfDay)
				},
				MultiplicatorDefinitionSet = multiplicatorDefinitionSet.Id.Value
			};

			var result = Target.Persist(overtimeRequestForm);

			Assert.AreEqual(result.IsPending, true);

			var newRequestStartDateTime = currentDateTime.AddDays(1);
			overtimeRequestForm.Id = Guid.Parse(result.Id);
			overtimeRequestForm.Period = new DateTimePeriodForm
			{
				StartDate = new DateOnly(newRequestStartDateTime),
				StartTime = new TimeOfDay(newRequestStartDateTime.TimeOfDay),
				EndDate = new DateOnly(newRequestStartDateTime),
				EndTime = new TimeOfDay(newRequestStartDateTime.AddHours(1).TimeOfDay)
			};

			Target.Persist(overtimeRequestForm);
			var denyCommand = _commandDispatcher.AllComands.OfType<DenyRequestCommand>().FirstOrDefault();
			Assert.IsNotNull(denyCommand);
			Assert.AreEqual(denyCommand.DenyReason, Resources.OvertimeRequestDenyReasonAutodeny);
		}
	}
}