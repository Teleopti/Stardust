using System;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;
using Percent = Teleopti.Interfaces.Domain.Percent;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPlanningTimeBankHelper
    {
        IPlanningTimeBankModel GetPlanningTimeBankModel(PersonDto personDto, DateOnlyDto dateOnlyDto);
        void SaveWantedBalanceOut(TimeSpan wantedBalanceOut);
    }

    public class PlanningTimeBankHelper : IPlanningTimeBankHelper
    {
        private readonly ITeleoptiSchedulingService _service;
        private PersonDto _personDto;
        private DateOnlyDto _dateOnlyDto;

        public PlanningTimeBankHelper(ITeleoptiSchedulingService service)
        {
            _service = service;
        }

        public IPlanningTimeBankModel GetPlanningTimeBankModel(PersonDto personDto, DateOnlyDto dateOnlyDto)
        {
            _personDto = personDto;
            _dateOnlyDto = dateOnlyDto;

            var planningTimeBankDto = _service.GetPlanningTimeBank(personDto, _dateOnlyDto);

            return new PlanningTimeBankModel(
                TimeSpan.FromMinutes(planningTimeBankDto.BalanceInMinutes),
                TimeSpan.FromMinutes(planningTimeBankDto.BalanceOutMinutes),
                TimeSpan.FromMinutes(planningTimeBankDto.BalanceOutMinMinutes),
                TimeSpan.FromMinutes(planningTimeBankDto.BalanceOutMaxMinutes),
                planningTimeBankDto.IsEditable);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetPlanningTimeBankModel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public void SaveWantedBalanceOut(TimeSpan wantedBalanceOut)
        {
            if(_personDto == null || _dateOnlyDto == null)
                throw new Exception("You must call GetPlanningTimeBankModel first to set person and date");

            _service.SavePlanningTimeBank(_personDto, _dateOnlyDto, (int)wantedBalanceOut.TotalMinutes, true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool SetIsAllowed(IPermissionService permissionService, IApplicationFunctionHelper appFunctionHelper, PersonDto personDto, DateOnlyDto dateOnlyDto, IPreferencePresenter preferencePresenter)
        {
            if (!permissionService.IsPermitted(appFunctionHelper.DefinedApplicationFunctionPaths.SetPlanningTimeBank))
                return false;

            var planningTimeBank = GetPlanningTimeBankModel(personDto, dateOnlyDto);
            if (!planningTimeBank.CanSetBalanceOut)
                return false;

            return preferencePresenter.HasOpenDays();
        }
    }
}