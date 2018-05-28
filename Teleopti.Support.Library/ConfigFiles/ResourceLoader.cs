using System.IO;
using System.Reflection;

namespace Teleopti.Support.Library.ConfigFiles
{
	public class ResourceLoader
	{
		public string Load(string name)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(ResourceLoader).Namespace + "." + name;
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
	}
}