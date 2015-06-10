using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class ReadDataSourceConfigurationFromNhibFiles : IReadDataSourceConfiguration
	{
		private readonly INhibFilePath _nhibFilePath;
		private readonly IParseNhibFile _parseNhibFile;
		private const string nhibFileType = "*.nhib.xml";

		public ReadDataSourceConfigurationFromNhibFiles(INhibFilePath nhibFilePath, IParseNhibFile parseNhibFile)
		{
			_nhibFilePath = nhibFilePath;
			_parseNhibFile = parseNhibFile;
		}

		public IDictionary<string, DataSourceConfiguration> Read()
		{
			var ret = new Dictionary<string, DataSourceConfiguration>();
			foreach (var nhibFile in Directory.GetFiles(_nhibFilePath.Path(), nhibFileType))
			{
				var dsCfg = _parseNhibFile.CreateDataSourceConfiguration(XDocument.Load(nhibFile));
				ret[dsCfg.Item1] = dsCfg.Item2;
			}
			return ret;
		}
	}
}