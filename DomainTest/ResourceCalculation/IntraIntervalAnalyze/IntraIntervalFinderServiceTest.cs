using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class IntraIntervalFinderServiceTest
	{
		private MockRepository _mock;
		private IntraIntervalFinderService _target;
		private ISkillDayIntraIntervalFinder _skillDayIntraIntervalFinder;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IResourceCalculationDataContainer _resourceCalculationDataContainer;
		private ISkillDay _skillDay;
		private ISkill _maxSeatSkill;
		private ISkill _backOfficeSkill;
		private ISkill _emailSkill;
		private ISkill _normalSkill;
		private double _limit;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_skillDayIntraIntervalFinder = _mock.StrictMock<ISkillDayIntraIntervalFinder>();
			_resourceCalculationDataContainer = _mock.StrictMock<IResourceCalculationDataContainer>();
			_skillDay = _mock.StrictMock<ISkillDay>();
			_target = new IntraIntervalFinderService(_skillDayIntraIntervalFinder);
			var skillTypeNormal = SkillTypeFactory.CreateSkillType();
			var skillTypeBackOffice = SkillTypeFactory.CreateSkillType();
			var skillTypeEmail = SkillTypeFactory.CreateSkillType();
			skillTypeNormal.ForecastSource = ForecastSource.MaxSeatSkill;
			skillTypeBackOffice.ForecastSource = ForecastSource.Backoffice;
			skillTypeEmail.ForecastSource = ForecastSource.Email;
			_maxSeatSkill = SkillFactory.CreateSkill("skill",skillTypeNormal,15);
			_backOfficeSkill = SkillFactory.CreateSkill("backoffice", skillTypeBackOffice, 60);
			_emailSkill = SkillFactory.CreateSkill("email", skillTypeEmail, 120);
			_normalSkill = SkillFactory.CreateSkill("skill");
			_limit = 0.7999;
		}

		[Test]
		public void ShouldNotSetIssuesOnMaxSeatSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {new DateOnly()})).Return(new List<ISkillDay>{_skillDay});
				Expect.Call(_skillDay.Skill).Return(_maxSeatSkill);
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}

		[Test]
		public void ShouldNotSetIssuesOnBackOfficeSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { new DateOnly() })).Return(new List<ISkillDay> { _skillDay });
				Expect.Call(_skillDay.Skill).Return(_backOfficeSkill);
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}

		[Test]
		public void ShouldNotSetIssuesOnEmailSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { new DateOnly() })).Return(new List<ISkillDay> { _skillDay });
				Expect.Call(_skillDay.Skill).Return(_emailSkill);
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}

		[Test]
		public void ShouldSetIssuesOnNormalSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { new DateOnly() })).Return(new List<ISkillDay> { _skillDay });
				Expect.Call(_skillDay.Skill).Return(_normalSkill);
				Expect.Call(() =>_skillDayIntraIntervalFinder.SetIntraIntervalIssues(_skillDay, _resourceCalculationDataContainer, _limit));
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}
	}
}
