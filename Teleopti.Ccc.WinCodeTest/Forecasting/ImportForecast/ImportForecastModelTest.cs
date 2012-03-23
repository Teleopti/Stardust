using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
    [TestFixture]
    public class ImportForecastModelTest
    {
        private IImportForecastModel _target;
        private MockRepository _mocks;
        private ISkill _skill;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IImportForecastsRepository _importForecastsRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _skill = SkillFactory.CreateSkillWithWorkloadAndSources();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _importForecastsRepository = _mocks.StrictMock<IImportForecastsRepository>();
            _target = new ImportForecastModel(_skill, _unitOfWorkFactory, _importForecastsRepository);
        }

        [Test]
        public void ShouldGetSkillName()
        {
            Assert.That(_target.GetSelectedSkillName(), Is.EqualTo("TestSkill"));   
        }

        [Test]
        public void ShouldLoadWorkload()
        {
            Assert.That(_target.LoadWorkload(), Is.EqualTo(_skill.WorkloadCollection.FirstOrDefault()));
        }

        [Test]
        public void ShouldSaveFileToServer()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var forecastFile = _mocks.StrictMock<IForecastFile>();
            _target.FileName = "C:\test.csv";
            var fileId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _importForecastsRepository.Add(forecastFile)).IgnoreArguments();
                Expect.Call(forecastFile.Id).Return(fileId);
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>{new RootChangeInfo(forecastFile, DomainUpdateType.Insert)});
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.SaveValidatedForecastFileInDb(), Is.EqualTo(fileId));
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldHandleInvalidatedFile()
        {
            _target.SaveValidatedForecastFileInDb();
        }
    }
}
