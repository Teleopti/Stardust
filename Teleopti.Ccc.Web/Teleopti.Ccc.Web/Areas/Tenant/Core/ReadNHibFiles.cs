using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ReadNHibFiles : IReadNHibFiles
	{
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly IParseNhibFile _parseNhibFile;

		public ReadNHibFiles(ISettings settings, 
												IPhysicalApplicationPath physicalApplicationPath,
												IParseNhibFile parseNhibFile)
		{
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
			_parseNhibFile = parseNhibFile;
		}

		public IDictionary<string, DataSourceConfiguration> Read()
		{
			var ret = new Dictionary<string, DataSourceConfiguration>();
			var nhibPath = _settings.nhibConfPath();
			var fullPathToNhibFolder = Path.Combine(_physicalApplicationPath.Get(), nhibPath);
			foreach (var nhibFile in Directory.GetFiles(fullPathToNhibFolder, "*.nhib.xml"))
			{
				var dsCfg = _parseNhibFile.CreateDataSourceConfiguration(XDocument.Load(nhibFile));
				ret[dsCfg.Tennant] = dsCfg;
			}
			return ret;
		}
	}
}