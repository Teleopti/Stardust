using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class ExportableScenarioProviderTest
    {
        private MockRepository _mocks;
        private IMeetingOverviewViewModel _model;
        private IScenarioRepository _scenarioRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ExportableScenarioProvider _target;
        private IUnitOfWork _unitOfWork;
        private IScenario _scenario1;
        private IScenario _scenario2;
        private IScenario _scenario3;
        private IList<IScenario> _scenarios;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IMeetingOverviewViewModel>();
            _scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target = new ExportableScenarioProvider(_model, _scenarioRepository, _unitOfWorkFactory);

            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            _scenario1 = _mocks.StrictMock<IScenario>();
            _scenario2 = _mocks.StrictMock<IScenario>();
            _scenario3 = _mocks.StrictMock<IScenario>();
            _scenarios = new List<IScenario> {_scenario1, _scenario2, _scenario3};
        }

        [Test]
        public void ShouldNotReturnRestrictedScenariosIfNoPermission()
        {
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork).Repeat.Twice();
            Expect.Call(_scenarioRepository.FindAllSorted()).Return(_scenarios);
            Expect.Call(() => _unitOfWork.Reassociate(_scenarios)).Repeat.Twice();
            Expect.Call(_scenario1.Restricted).Return(true);
            Expect.Call(_scenario2.Restricted).Return(false);
            Expect.Call(_scenario3.Restricted).Return(false);
            Expect.Call(_model.CurrentScenario).Return(_scenario3).Repeat.Twice();
            Expect.Call(() => _unitOfWork.Dispose()).Repeat.Twice();
            _mocks.ReplayAll();
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                _target.AllowedScenarios().Count.Should().Be.EqualTo(1);
            }

            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
            {
                _target.AllowedScenarios().Count.Should().Be.EqualTo(2);
            }
            _mocks.VerifyAll();
        }
    }

}