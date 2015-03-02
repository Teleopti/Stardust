using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class CreateSkillsFromMaxSeatSitesTest
	{
		private ICreateSkillsFromMaxSeatSites _target;
		private ISite _site1;
		private ISite _site2;
	    private MockRepository _mocks;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;


	    [SetUp]
		public void Setup()
		{
		    _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _target = new CreateSkillsFromMaxSeatSites(_schedulingResultStateHolder);
			_site1 = SiteFactory.CreateSimpleSite("site1");
			_site1.MaxSeats = 20;
			_site2 = SiteFactory.CreateSimpleSite("site2");
		}

		[Test]
		public void ShouldCreateSkillForSiteWithMaxSeat()
		{
		    var skills = new List<ISkill>();
		    Expect.Call(_schedulingResultStateHolder.Skills).Return(skills);
            _mocks.ReplayAll();
			IList<ISite> sites = new List<ISite> {_site1, _site2};
			_target.CreateSkillList(sites);
            Assert.AreEqual(1, skills.Count);
            Assert.AreEqual(skills[0], _site1.MaxSeatSkill);
            Assert.AreEqual(15, skills[0].DefaultResolution);
            Assert.AreEqual(_site1.Description.Name, skills[0].Name);
            Assert.AreEqual(ForecastSource.MaxSeatSkill, skills[0].SkillType.ForecastSource);
            Assert.AreEqual(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone.Id, skills[0].TimeZone.Id);
            Assert.AreEqual(96, skills[0].GetTemplateAt(0).TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(20, skills[0].GetTemplateAt(0).TemplateSkillDataPeriodCollection[0].MaxSeats);
			_mocks.VerifyAll();
            //openhours?
			//Color?
		}
	}
}