using System.IO;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettingsTenantClientForNoWeb : ISharedSettingsTenantClient
	{
		private readonly string _path;

		public SharedSettingsTenantClientForNoWeb(string path)
		{
			_path = path;
		}

		public SharedSettings GetSharedSettings()
		{
			var json = File.ReadAllText(_path);
			return JsonConvert.DeserializeObject<SharedSettings>(json);
		}
	}
}