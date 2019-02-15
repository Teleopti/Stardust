using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class State : IState
    {
        private IApplicationData _applicationScopeData;

	    public virtual void SetApplicationData(IApplicationData applicationData)
		{
			_applicationScopeData = applicationData;
		}

		public virtual IApplicationData ApplicationScopeData_DONTUSE => _applicationScopeData;
	}
}