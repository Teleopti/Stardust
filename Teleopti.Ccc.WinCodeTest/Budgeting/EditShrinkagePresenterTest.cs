using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class EditShrinkagePresenterTest
    {
        private MockRepository _mock;
        private IEditShrinkageForm _view;
        private EditShrinkageModel _model;
        private EditShrinkagePresenter _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        private ICustomShrinkage _shrinkage;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _view = _mock.StrictMock<IEditShrinkageForm>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mock.StrictMock<IRepositoryFactory>();
            var budgetGroup = new BudgetGroup();
            _shrinkage = new CustomShrinkage("Vacation");
            budgetGroup.AddCustomShrinkage(_shrinkage);
            _model = _mock.StrictMock<EditShrinkageModel>(_shrinkage, _unitOfWorkFactory, _repositoryFactory);
            _target = new EditShrinkagePresenter(_view, _model);
        }

        [Test]
        public void ShouldInitialize()
        {
            var absences = new List<IAbsence> {AbsenceFactory.CreateAbsence("Holiday")};
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var absenceRepository = _mock.StrictMock<IAbsenceRepository>();
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepository);
                Expect.Call(absenceRepository.LoadAll()).Return(absences);
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Initialize();
                Assert.That(_target.Absences.Count(), Is.EqualTo(absences.Count));
                Assert.That(_target.AddedAbsences.Count(), Is.EqualTo(0));
                Assert.That(_target.ShrinkageName, Is.EqualTo("Vacation"));
                Assert.That(_target.IncludedInAllowance, Is.False);
                Assert.That(_target.ShrinkageId, Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void ShouldRemoveAllSelectedAbsences()
        {
            using(_mock.Record())
            {
                Expect.Call(_view.RemoveSelectedAbsences);
            }
            using(_mock.Playback())
            {
                _target.RemoveAllAbsences();
            }
        }

        [Test]
        public void ShouldAddSelectedAbsences()
        {
            using(_mock.Record())
            {
                Expect.Call(_view.AddSelectedAbsences);
            }
            using(_mock.Playback())
            {
                _target.AddAbsences();
            }
        }

        [Test]
        public void ShouldSaveCustomShrinkage()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _model.Save(_shrinkage));
            }
            using(_mock.Playback())
            {
                _target.Save(_shrinkage);
            }
        }
    }
}
