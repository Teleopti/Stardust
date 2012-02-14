using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class PlanningTimeBankHelperTest
    {
        private MockRepository _mocks;
        private ITeleoptiSchedulingService _service;
        private PlanningTimeBankHelper _target;
        private PersonDto _personDto;
        private DateOnly _dateOnly;
        private PlanningTimeBankDto _dto;
        private DateOnlyDto _dateOnlyDto;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _service = _mocks.StrictMock<ITeleoptiSchedulingService>();
            _target = new PlanningTimeBankHelper(_service);
            _personDto = new PersonDto();
            _dateOnly = new DateOnly(2011,02,15);
            _dateOnlyDto = new DateOnlyDto { DateTime = _dateOnly.Date, DateTimeSpecified = true };
            _dto = new PlanningTimeBankDto
            {
                BalanceInMinutes = 5 * 60,
                BalanceInMinutesSpecified = true,
                BalanceOutMaxMinutes = 40 * 60,
                BalanceOutMaxMinutesSpecified = true,
                BalanceOutMinMinutes = -5 * 60,
                BalanceOutMinMinutesSpecified = true,
                BalanceOutMinutes = 2 * 60,
                BalanceOutMinutesSpecified = true,
                IsEditable = true,
                IsEditableSpecified = true
            };
        }

        [Test]
        public void ShouldCallServiceForPlanningTimeBank()
        {
            Expect.Call(_service.GetPlanningTimeBank(_personDto, _dateOnlyDto)).Return(_dto);
            _mocks.ReplayAll();
            var result = _target.GetPlanningTimeBankModel(_personDto, _dateOnlyDto);
            Assert.That(result.TimeBankMax, Is.EqualTo(TimeSpan.FromMinutes(40*60)));
            Assert.That(result.TimeBankMin, Is.EqualTo(TimeSpan.FromMinutes(-5*60)));
            Assert.That(result.BalanceOut, Is.EqualTo(TimeSpan.FromMinutes(2*60)));
            Assert.That(result.BalanceIn, Is.EqualTo(TimeSpan.FromMinutes(5*60)));
            Assert.That(result.CanSetBalanceOut, Is.True);
            
            _mocks.VerifyAll();
        }

        [Test, ExpectedException]
        public void ShouldThrowIfGetIsNotCalledFirst()
        {
            _target.SaveWantedBalanceOut(TimeSpan.FromMinutes(500));
        }

        [Test]
        public void ShouldCallServiceAndSave()
        {
            Expect.Call(_service.GetPlanningTimeBank(_personDto, _dateOnlyDto)).Return(_dto);
            Expect.Call(() =>_service.SavePlanningTimeBank(_personDto, _dateOnlyDto, 500 ,true));
            _mocks.ReplayAll();
            _target.GetPlanningTimeBankModel(_personDto, _dateOnlyDto);
            _target.SaveWantedBalanceOut(TimeSpan.FromMinutes(500));
        }

        [Test]
        public void SetIsAllowedShouldReturnFalseOnNoRights()
        {
            var permissionService = _mocks.StrictMock<IPermissionService>();
            var appFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            var preferencePresenter = _mocks.StrictMock<IPreferencePresenter>();
            var dto = new DefinedRaptorApplicationFunctionPathsDto
                          {
                              SetPlanningTimeBank = "Raptor/AgentPortal/SetPlanningTimeBank"
                          };
            Expect.Call(appFunctionHelper.DefinedApplicationFunctionPaths).Return(dto);
            Expect.Call(permissionService.IsPermitted("Raptor/AgentPortal/SetPlanningTimeBank")).Return(false);
            
            _mocks.ReplayAll();
            bool result = _target.SetIsAllowed(permissionService, appFunctionHelper, _personDto, _dateOnlyDto, preferencePresenter);
            Assert.That(result,Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void SetIsAllowedShouldReturnFalseIfNotFirstPeriod()
        {
            var permissionService = _mocks.StrictMock<IPermissionService>();
            var appFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            var preferencePresenter = _mocks.StrictMock<IPreferencePresenter>();
            var dto = new DefinedRaptorApplicationFunctionPathsDto
            {
                SetPlanningTimeBank = "Raptor/AgentPortal/SetPlanningTimeBank"
            };
            _dto.IsEditable = false;
            Expect.Call(appFunctionHelper.DefinedApplicationFunctionPaths).Return(dto);
            Expect.Call(permissionService.IsPermitted("Raptor/AgentPortal/SetPlanningTimeBank")).Return(true);
            Expect.Call(_service.GetPlanningTimeBank(_personDto, _dateOnlyDto)).Return(_dto);
            _mocks.ReplayAll();
            bool result = _target.SetIsAllowed(permissionService, appFunctionHelper,_personDto, _dateOnlyDto,preferencePresenter);
            Assert.That(result, Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void SetIsAllowedShouldReturnFalseIfNoOpenDays()
        {
            var permissionService = _mocks.StrictMock<IPermissionService>();
            var appFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            var preferencePresenter = _mocks.StrictMock<IPreferencePresenter>();
            var dto = new DefinedRaptorApplicationFunctionPathsDto
            {
                SetPlanningTimeBank = "Raptor/AgentPortal/SetPlanningTimeBank"
            };
            
            Expect.Call(appFunctionHelper.DefinedApplicationFunctionPaths).Return(dto);
            Expect.Call(permissionService.IsPermitted("Raptor/AgentPortal/SetPlanningTimeBank")).Return(true);
            Expect.Call(_service.GetPlanningTimeBank(_personDto, _dateOnlyDto)).Return(_dto);
            Expect.Call(preferencePresenter.HasOpenDays()).Return(false);
            _mocks.ReplayAll();
            bool result = _target.SetIsAllowed(permissionService, appFunctionHelper, _personDto, _dateOnlyDto, preferencePresenter);
            Assert.That(result, Is.False);
            _mocks.VerifyAll();
        }
        
        [Test]
        public void SetIsAllowedShouldReturnTrueIfOpenDays()
        {
            var permissionService = _mocks.StrictMock<IPermissionService>();
            var appFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            var preferencePresenter = _mocks.StrictMock<IPreferencePresenter>();
            var dto = new DefinedRaptorApplicationFunctionPathsDto
            {
                SetPlanningTimeBank = "Raptor/AgentPortal/SetPlanningTimeBank"
            };

            Expect.Call(appFunctionHelper.DefinedApplicationFunctionPaths).Return(dto);
            Expect.Call(permissionService.IsPermitted("Raptor/AgentPortal/SetPlanningTimeBank")).Return(true);
            Expect.Call(_service.GetPlanningTimeBank(_personDto, _dateOnlyDto)).Return(_dto);
            Expect.Call(preferencePresenter.HasOpenDays()).Return(true);
            _mocks.ReplayAll();
            bool result = _target.SetIsAllowed(permissionService, appFunctionHelper, _personDto, _dateOnlyDto, preferencePresenter);
            Assert.That(result, Is.True);
            _mocks.VerifyAll();
        }
    }

    
}