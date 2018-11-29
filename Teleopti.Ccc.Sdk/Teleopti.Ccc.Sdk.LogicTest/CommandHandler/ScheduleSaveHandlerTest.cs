using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	class ScheduleSaveHandlerTest
	{
		private ISaveSchedulePartService _saveSchedulePartService;
		private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private readonly DateTimePeriod _period = new DateTimePeriod(_startDate, _startDate.AddDays(1));
		private IPerson _person;
		private IScenario _scenario;
		private IScheduleDay _scheduleDay;
		private INewBusinessRuleCollection _rules;
		private ScheduleTag _scheduleTag;

		[SetUp]
		public void Setup()
		{
			_saveSchedulePartService = MockRepository.GenerateMock<ISaveSchedulePartService>();
			_person = PersonFactory.CreatePerson("test");
			_person.SetId(Guid.NewGuid());

			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_scenario.SetId(Guid.NewGuid());
			var scheduleRange = new SchedulePartFactoryForDomain(_person, _scenario, _period, SkillFactory.CreateSkill("Test Skill"));
			_scheduleDay = scheduleRange.CreatePart();
			_rules = MockRepository.GenerateMock<INewBusinessRuleCollection>();
			_scheduleTag = new ScheduleTag();
		}

		[Test]
		public void ShouldSaveSchedule()
		{
			var target = new ScheduleSaveHandler(_saveSchedulePartService);
			target.ProcessSave(_scheduleDay, _rules, _scheduleTag);

			_saveSchedulePartService.AssertWasCalled(x => x.Save(_scheduleDay, _rules, _scheduleTag));
		}

		[Test]
		public void ShouldThrowExceptionWhenReturnErrorMessage()
		{
			var expectedError = new List<string>() {"error"};
			_saveSchedulePartService.Stub(x=>x.Save(_scheduleDay, _rules, _scheduleTag)).Return(expectedError);

			var target = new ScheduleSaveHandler(_saveSchedulePartService);

			var expectErrorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
					string.Join(Environment.NewLine, expectedError));

			try
			{
				target.ProcessSave(_scheduleDay, _rules, _scheduleTag);
			}
			catch (FaultException e)
			{
				e.Message.Should().Be.EqualTo(expectErrorMessage);
			}
		}
	}
}
