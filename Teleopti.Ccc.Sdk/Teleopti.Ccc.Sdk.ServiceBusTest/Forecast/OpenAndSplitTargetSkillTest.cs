using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
    [TestFixture]
    public class OpenAndSplitTargetSkillTest
    {
        private MockRepository _mocks;
		private ICurrentUnitOfWorkFactory _currentunitOfWorkFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISkillRepository _skillRepository;
        private IJobResultRepository _jobResultRepository;
        private IJobResultFeedback _feedback;
        private IMessageBroker _messageBroker;
        private IServiceBus _serviceBus;
        private IOpenAndSplitSkillCommand _command;
        private OpenAndSplitTargetSkillConsumer _target;
        private IUnitOfWork _unitOfWork;
        private IJobResult _jobResult;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_currentunitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _skillRepository = _mocks.StrictMock<ISkillRepository>();
            _jobResultRepository = _mocks.StrictMock<IJobResultRepository>();
            _feedback = _mocks.DynamicMock<IJobResultFeedback>();
            _messageBroker = _mocks.DynamicMock<IMessageBroker>();
            _serviceBus = _mocks.StrictMock<IServiceBus>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _jobResult = _mocks.DynamicMock<IJobResult>();
            _command = _mocks.StrictMock<IOpenAndSplitSkillCommand>();
			_target = new OpenAndSplitTargetSkillConsumer(_currentunitOfWorkFactory, _command, _skillRepository,
                                                          _jobResultRepository, _feedback, _messageBroker,
                                                          _serviceBus);
        }

        [Test]
        public void ShouldHandleMessageCorrectly()
        {
            var dateTime = new DateOnly(2012, 3, 1);
            var jobId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var skill = SkillFactory.CreateSkill("test skill");
            using (_mocks.Record())
            {
				Expect.Call(_currentunitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_jobResultRepository.Get(jobId)).Return(_jobResult);
                Expect.Call(_skillRepository.Get(skillId)).Return(skill);
                Expect.Call(()=>_command.Execute(skill, new DateOnlyPeriod(dateTime,dateTime),new[]{new TimePeriod(8,0,17,0)}));
                Expect.Call(() => _serviceBus.Send()).Constraints(
                    Rhino.Mocks.Constraints.Is.Matching<Object[]>(a => ((ImportForecastsToSkill)a[0]).Date == dateTime));
            }
            using (_mocks.Playback())
            {
                var message = new OpenAndSplitTargetSkill
                                  {
                                      JobId = jobId,
                                      ImportMode = ImportForecastsMode.ImportWorkload,
                                      TargetSkillId = skillId,
                                      Date = dateTime,
                                      StartOpenHour = TimeSpan.FromHours(8),
                                      EndOpenHour = TimeSpan.FromHours(17),
                                      Timestamp = DateTime.Now
                                  };
                _target.Consume(message);
            }
        }

    }
}
