using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
	[TestFixture]
	public class ReportSettingsScheduleTimeVersusTargetTimeModelTest
	{
		private ReportSettingsScheduleTimeVersusTargetTimeModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new ReportSettingsScheduleTimeVersusTargetTimeModel();
		}

		//[Test]
		//public void ShouldGetSetGroupPage()
		//{
		//    IGroupPage groupPage = new GroupPage("groupPage");
		//    _model.GroupPage = groupPage;

		//    Assert.AreEqual(groupPage, _model.GroupPage);
		//}

		//[Test]
		//public void ShouldGetSetSite()
		//{
		//    ISite site = new Site("site");
		//    _model.Site = site;

		//    Assert.AreEqual(site, _model.Site);
		//}

		//[Test]
		//public void ShouldGetSetTeam()
		//{
		//    ITeam team = new Team();
		//    _model.Team = team;

		//    Assert.AreEqual(team, _model.Team);
		//}

		[Test]
		public void ShouldGetSetAgents()
		{
			IPerson person = new Person();
			IList<IPerson> persons = new List<IPerson>{person};

			_model.SetPersons(persons);

			Assert.AreEqual(1, _model.Persons.Count);
			Assert.AreEqual(person, _model.Persons.First());
		}

		[Test]
		public void ShouldGetSetScenario()
		{
			IScenario scenario = new Scenario("scenario");
			_model.Scenario = scenario;

			Assert.AreEqual(scenario, _model.Scenario);
		}

		[Test]
		public void ShouldGetSetPeriod()
		{
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 2);
			_model.Period = period;

			Assert.AreEqual(period, _model.Period);
		}
	}
}
