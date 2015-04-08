using System.IO;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Infrastructure.Web
{
	public class SharedSettingsQuerierForNoWeb : ISharedSettingsQuerier
	{
		private readonly string _path;

		public SharedSettingsQuerierForNoWeb(string path)
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