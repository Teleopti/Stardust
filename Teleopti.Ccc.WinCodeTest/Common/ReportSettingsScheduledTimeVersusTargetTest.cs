using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Reporting;


namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ReportSettingsScheduledTimeVersusTargetTest
	{
		private ReportSettingsScheduledTimeVersusTarget _settings;
		private IPerson _person1;
		private IPerson _person2;
		private IScenario _scenario;
		private DateTimePeriod _period;
		private string _groupPage;

		[SetUp]
		public void Setup()
		{
			_person1 = PersonFactory.CreatePerson("person1");
			_person2 = PersonFactory.CreatePerson("person2");
			_person1.SetId(Guid.NewGuid());
			_person2.SetId(Guid.NewGuid());
			_scenario = new Scenario("scenario");
			_scenario.SetId(Guid.NewGuid());
			_period = new DateTimePeriod(2011, 1, 1, 2011, 1, 31);
			_groupPage = "pageKey";
			_settings = new ReportSettingsScheduledTimeVersusTarget();
		}

		[Test]
		public void ShouldGetSetPersons()
		{
			Assert.IsTrue(_person1.Id.HasValue);
			Assert.IsTrue(_person2.Id.HasValue);

			IList<Guid> personIDs = new List<Guid> {_person1.Id.Value, _person2.Id.Value};

			_settings.Persons = personIDs;
			Assert.AreEqual(personIDs, _settings.Persons);
		}

		[Test]
		public void ShouldGetSetGroupPage()
		{
			_settings.GroupPage = _groupPage;

			Assert.AreEqual(_groupPage, _settings.GroupPage);
		}

		[Test]
		public void ShouldGetSetScenario()
		{
			Assert.IsTrue(_scenario.Id.HasValue);

			_settings.Scenario = _scenario.Id.Value;
			Assert.AreEqual(_scenario.Id, _settings.Scenario);
		}

		[Test]
		public void ShouldGetSetPeriod()
		{
			_settings.StartDate = _period.StartDateTime;
			_settings.EndDate = _period.EndDateTime;

			Assert.AreEqual(_period.StartDateTime, _settings.StartDate);
			Assert.AreEqual(_period.EndDateTime, _settings.EndDate);
		}
	}
}
