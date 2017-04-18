using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportAcrossBusinessUnitsSettingsProviderTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IExportAcrossBusinessUnitsSettingsProvider _target;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new ExportAcrossBusinessUnitsSettingsProvider(_unitOfWorkFactory, _repositoryFactory);
        }

        [Test]
        public void ShouldLoadSettings()
        {
            var exportAcrossBusinessUnitsSettings = _mocks.DynamicMock<IExportAcrossBusinessUnitsSettings>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(
                    _repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey
                        <IExportAcrossBusinessUnitsSettings>(
                            "ExportAcrossBusinessUnitsSettings", new ExportAcrossBusinessUnitsSettings())).Return(
                                exportAcrossBusinessUnitsSettings).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
              Assert.That(_target.ExportAcrossBusinessUnitsSettings, Is.EqualTo(exportAcrossBusinessUnitsSettings));
            }
        }

        [Test]
        public void ShouldSaveSettings()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            var settings = _mocks.DynamicMock<IExportAcrossBusinessUnitsSettings>();
            var globalSettingRepository = _mocks.StrictMock<ISettingDataRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.Twice();
                Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(globalSettingRepository).Repeat.Twice();
                Expect.Call(globalSettingRepository.FindValueByKey<IExportAcrossBusinessUnitsSettings>(
                                    "ExportAcrossBusinessUnitsSettings", new ExportAcrossBusinessUnitsSettings())).
                    Return(settings).IgnoreArguments();
                Expect.Call(globalSettingRepository.PersistSettingValue(null)).IgnoreArguments().Return(null);
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
            using (_mocks.Playback())
            {
                _target.Save();
            }
        }

        [Test]
        public void ShouldTransformSerializableToSelectionModels()
        {
            var multisiteSkillId = Guid.NewGuid();
            var sourceSkillId = Guid.NewGuid();
            var targetSkillId = Guid.NewGuid();
            var multisiteSkillSelections = new Dictionary<Guid, IEnumerable<ChildSkillSelectionMapping>>
                                               {
                                                   {
                                                       multisiteSkillId, new List<ChildSkillSelectionMapping>
                                                                             {
                                                                                 new ChildSkillSelectionMapping
                                                                                     {
                                                                                         SourceSkillId = sourceSkillId,
                                                                                         TargetSkillId = targetSkillId,
                                                                                         TargetBuName = string.Empty,
                                                                                         TargetSkillName = string.Empty
                                                                                     }
                                                                             }
                                                       }
                                               };
            var multisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
            var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel
                                                   {MultisiteSkillModel = new MultisiteSkillModel(multisiteSkillId)};
            multisiteSkillSelectionModel.ChildSkillMappingModels.Add(new ChildSkillMappingModel(sourceSkillId,
                                                                                                targetSkillId,
                                                                                                string.Empty,
                                                                                                string.Empty));
            
            multisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);

            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var settings = new ExportAcrossBusinessUnitsSettings { MultisiteSkillSelections = multisiteSkillSelections};
            using(_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey<IExportAcrossBusinessUnitsSettings>(
                        "ExportAcrossBusinessUnitsSettings", new ExportAcrossBusinessUnitsSettings())).Return(
                            settings).IgnoreArguments();
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mocks.Playback())
            {
                Assert.That(_target.TransformSerializableToSelectionModels().Count(), Is.EqualTo(multisiteSkillSelectionModels.Count));
                Assert.That(_target.TransformSerializableToSelectionModels().First().MultisiteSkillModel.Id,
                            Is.EqualTo(multisiteSkillSelectionModels.First().MultisiteSkillModel.Id));
            }
        }

        [Test]
        public void ShouldTransformToSerializableModel()
        {
            var multisiteSkillId = Guid.NewGuid();
            var sourceSkillId = Guid.NewGuid();
            var targetSkillId = Guid.NewGuid();
            var multisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
            var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel { MultisiteSkillModel = new MultisiteSkillModel(multisiteSkillId) };
            multisiteSkillSelectionModel.ChildSkillMappingModels.Add(new ChildSkillMappingModel(sourceSkillId,
                                                                                                targetSkillId,
                                                                                                string.Empty,
                                                                                                string.Empty));
            multisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);
            var settings = new ExportAcrossBusinessUnitsSettings();
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey<IExportAcrossBusinessUnitsSettings>(
                        "ExportAcrossBusinessUnitsSettings", new ExportAcrossBusinessUnitsSettings())).Return(
                            settings).IgnoreArguments();
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mocks.Playback())
            {
                _target.TransformToSerializableModel(multisiteSkillSelectionModels);

                Assert.That(_target.ExportAcrossBusinessUnitsSettings.MultisiteSkillSelections.Count, Is.EqualTo(1));
                Assert.That(
                    _target.ExportAcrossBusinessUnitsSettings.MultisiteSkillSelections.First().Value.First().
                        SourceSkillId, Is.EqualTo(sourceSkillId));
            }
        }
    }
}
