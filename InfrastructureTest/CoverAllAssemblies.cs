using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Teleopti.Ccc.InfrastructureTest
{
	[TestFixture]
	public class CoverAllAssemblies
	{
		[Test]
		public void MakeSureEveryAssemblyIsIncludedInCoverageReport()
		{
			var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent;
			var dlls = dirInfo.GetFiles("Teleopti.*.dll", SearchOption.AllDirectories)
				.Union(dirInfo.GetFiles("Teleopti.*.exe", SearchOption.AllDirectories));
			var dllsAlreadyAdded = new List<string>();

			foreach (var fileInfo in dlls)
			{
				var fileName = fileInfo.Name;
				if (!dllsAlreadyAdded.Contains(fileName))
				{
					dllsAlreadyAdded.Add(fileName);
					Assembly.LoadFile(fileInfo.FullName);
				}
			}
		}
	}
}