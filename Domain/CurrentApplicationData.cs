using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain
{
	public class CurrentApplicationData : ICurrentApplicationData
	{
		private readonly Func<IApplicationData> _applicationData;

		public CurrentApplicationData(Func<IApplicationData> applicationData)
		{
			_applicationData = applicationData;
		}

		public IApplicationData Current()
		{
			return _applicationData.Invoke();
		}
	}
}