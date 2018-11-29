using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	public class CreateOrUpdateSkillDaysTest
	{
		private CreateOrUpdateSkillDays _target;
		private IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private IForecastingTargetMerger _forecastingTargetMerger;
		private ISkillDayRepository _skillDayRepository;
		private ISkill _skill;
		private ISkillDay _skillDay;
		private IWorkloadDay _workLoadDay;
		private IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;

		[SetUp]
		public void Setup()
		{
			var outboundProductionPlanFactory =
				new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			_fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			_forecastingTargetMerger = MockRepository.GenerateMock<IForecastingTargetMerger>();
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_outboundScheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_target = new CreateOrUpdateSkillDays(outboundProductionPlanFactory, _fetchAndFillSkillDays, _forecastingTargetMerger,
				_skillDayRepository, _outboundScheduledResourcesProvider);
			_skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			_skillDay = MockRepository.GenerateMock<ISkillDay>();
			_workLoadDay = MockRepository.GenerateMock<IWorkloadDay>();
		}

		[Test]
		public void ShouldWork()
		{
			var skillDayList = new List<ISkillDay> {_skillDay};
			_fetchAndFillSkillDays.Stub(x => x.FindRange(new DateOnlyPeriod(2015, 6, 15, 2015, 6, 15), _skill)).Return(skillDayList);
			_skillDay.Stub(x => x.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new List<IWorkloadDay> {_workLoadDay}));
			_workLoadDay.Stub(x => x.TemplateReference).Return(new TemplateReference());
			_target.Create(_skill, new DateOnlyPeriod(2015, 6, 15, 2015, 6, 15), 100, TimeSpan.FromMinutes(100),
				new Dictionary<DayOfWeek, TimePeriod>());
			
			_skillDayRepository.AssertWasCalled(x => x.AddRange(skillDayList));
		}
	}
}