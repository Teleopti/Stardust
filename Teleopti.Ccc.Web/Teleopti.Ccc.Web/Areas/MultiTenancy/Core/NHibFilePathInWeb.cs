using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class NHibFilePathInWeb : INhibFilePath
	{
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;

		public NHibFilePathInWeb(ISettings settings, IPhysicalApplicationPath physicalApplicationPath)
		{
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
		}

		public string Path()
		{
			var nhibPath = _settings.nhibConfPath();
			return System.IO.Path.Combine(_physicalApplicationPath.Get(), nhibPath);
		}
	}
}