using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain
{
	public class ThisApplicationData : ICurrentApplicationData
	{
		private readonly IApplicationData _applicationData;

		public ThisApplicationData(IApplicationData applicationData)
		{
			_applicationData = applicationData;
		}

		public IApplicationData Current()
		{
			return _applicationData;
		}
	}
}