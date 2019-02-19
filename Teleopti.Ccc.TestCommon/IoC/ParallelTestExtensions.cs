using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public static class ParallelTestExtensions
	{
		public static bool isParallel(this TestContext instance) => 
			new Regex(@"Worker#[0-9]+")
				.IsMatch(instance.WorkerId);
	}
}