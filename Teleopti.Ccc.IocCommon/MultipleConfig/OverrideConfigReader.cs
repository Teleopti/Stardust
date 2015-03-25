using System.Collections.Generic;
using System.IO;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class OverrideConfigReader
	{
		private readonly string _directory;

		public OverrideConfigReader(string directory)
		{
			_directory = directory;
		}

		public IDictionary<string, string> Overrides()
		{
			const string overrideConfigFilePatter = "*.override.config";

			var ret = new Dictionary<string, string>();
			foreach (var overrideFile in Directory.EnumerateFiles(_directory, overrideConfigFilePatter))
			{
				ret[Path.GetFileName(overrideFile).Replace(".override.config", string.Empty)] = overrideFile;
			}

			return ret;
		}
	}
}