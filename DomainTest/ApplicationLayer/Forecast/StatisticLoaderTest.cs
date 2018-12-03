using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class StatisticLoaderTest
	{
		private MockRepository _mocks;
		private StatisticLoader _target;
		private IRepositoryFactory _repositoryFactory;
		private IWorkloadDay _workloadDay;
		private IWorkload _workload;
		private IStatistic _statistic;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_repositoryFactory = _mocks.DynamicMock<IRepositoryFactory>();
			_statistic = _mocks.DynamicMock<IStatistic>();
			_target = new StatisticLoader(_repositoryFactory, _statistic);

			_workloadDay = _mocks.DynamicMock<IWorkloadDay>();
			_workload = _mocks.DynamicMock<IWorkload>();
		}

		[Test]
		public void ShouldLoadFromDatabase()
		{
			var date= new DateTime(2012, 12, 18, 14, 0, 0, DateTimeKind.Utc);
			var task = new Task();
			var taskPeriod = new TemplateTaskPeriod(task, new DateTimePeriod(date, date.AddMinutes(15)));
			var statTask = new StatisticTask{Interval = date};
			var serviceAgreement = new ServiceAgreement();
			var skillStaffPeriod = new SkillStaffPeriod(new DateTimePeriod(date, date.AddMinutes(15)), task, serviceAgreement){StatisticTask = statTask};
			Expect.Call(_workloadDay.Workload).Return(_workload);
			Expect.Call(_workload.QueueSourceCollection).Return(new ReadOnlyCollection<IQueueSource>(new List<IQueueSource>()));
			Expect.Call(_repositoryFactory.CreateStatisticRepository().LoadSpecificDates(new Collection<IQueueSource>(), new DateTimePeriod())).
				IgnoreArguments().Return(new Collection<IStatisticTask> { statTask });
			Expect.Call(_workloadDay.OpenTaskPeriodList).Return(
				new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { taskPeriod }));
			_mocks.ReplayAll();
			_target.Execute(new DateTimePeriod(), _workloadDay, new List<ISkillStaffPeriod> { skillStaffPeriod });
			_mocks.VerifyAll();
		}
	}
}