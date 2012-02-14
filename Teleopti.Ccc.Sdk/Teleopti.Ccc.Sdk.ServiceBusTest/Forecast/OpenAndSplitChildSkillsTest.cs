using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class OpenAndSplitChildSkillsTest
	{
		private IUnitOfWorkFactory unitOfWorkFactory;
		private ISkillRepository skillRepository;
		private OpenAndSplitChildSkillsConsumer target;
		private MockRepository mocks;
		private IJobResultRepository jobResultRepository;
		private IJobResultFeedback jobResultFeedback;
		private IMessageBroker messageBroker;
		private IOpenAndSplitSkillCommand command;
		private IServiceBus serviceBus;
		private ISkill skill;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			skillRepository = mocks.DynamicMock<ISkillRepository>();
			jobResultRepository = mocks.DynamicMock<IJobResultRepository>();
			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			messageBroker = mocks.DynamicMock<IMessageBroker>();
			serviceBus = mocks.DynamicMock<IServiceBus>();
			command = mocks.DynamicMock<IOpenAndSplitSkillCommand>();
			skill = SkillFactory.CreateSkillWithWorkloadAndSources();

			target = new OpenAndSplitChildSkillsConsumer(unitOfWorkFactory, command, skillRepository, jobResultRepository, jobResultFeedback, messageBroker, serviceBus);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldHandleMessageCorrectly()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var disposable = mocks.DynamicMock<IDisposable>();
			var jobResult = mocks.DynamicMock<IJobResult>();
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 31);
			var jobId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(unitOfWork.DisableFilter(QueryFilter.BusinessUnit)).Return(disposable);
				Expect.Call(skillRepository.Get(Guid.Empty)).Return(skill);
				Expect.Call(() => command.Execute(skill, period));
				Expect.Call(jobResultRepository.Get(jobId)).Return(jobResult);
				Expect.Call(() => jobResultFeedback.SetJobResult(jobResult, messageBroker));
				Expect.Call(() => serviceBus.Send()).Constraints(
					Rhino.Mocks.Constraints.Is.Matching<Object[]>(a => ((ExportMultisiteSkillToSkill)a[0]).Period == period));
			}
			using (mocks.Playback())
			{
				var skillSelection = new MultisiteSkillSelection();
				skillSelection.ChildSkillSelections.Add(new ChildSkillSelection());
				target.Consume(new OpenAndSplitChildSkills { JobId = jobId, MultisiteSkillSelections = skillSelection, Period = period });
			}
		}
	}
}