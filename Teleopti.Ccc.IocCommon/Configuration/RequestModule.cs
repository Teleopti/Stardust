using System.Threading;
using Autofac;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class RequestModule : Module
    {
		private readonly IIocConfiguration _configuration;


		public RequestModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

	    protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestFactory>().As<IRequestFactory>();
			builder.RegisterType<PersonRequestCheckAuthorization>().As<IPersonRequestCheckAuthorization>();
			builder.RegisterType<BudgetGroupAllowanceSpecification>().As<IBudgetGroupAllowanceSpecification>();
			builder.RegisterType<AlreadyAbsentSpecification>().As<IAlreadyAbsentSpecification>();
			builder.RegisterType<AbsenceRequestUpdater>().As<IAbsenceRequestUpdater>().SingleInstance();
			builder.RegisterType<AbsenceRequestWaitlistProvider>().As<IAbsenceRequestWaitlistProvider>();
			builder.RegisterType<AbsenceRequestWaitlistProcessor>().As<IAbsenceRequestWaitlistProcessor>().SingleInstance();
			builder.RegisterType<AbsenceRequestProcessor>().As<IAbsenceRequestProcessor>().SingleInstance();

			//ROBTODO: remove when Wfm_Requests_Cancel_37741 is always true
			if (_configuration.Toggle (Toggles.Wfm_Requests_Cancel_37741))
		    {
				builder.RegisterType<AbsenceRequestCancelService>().As<IAbsenceRequestCancelService>().SingleInstance();
			}
		    else
		    {
				builder.RegisterType<AbsenceRequestCancelServiceWfmRequestsCancel37741ToggleOff>().As<IAbsenceRequestCancelService>().SingleInstance();
			}
			
		}
    }
}