using System.Globalization;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	public class TestCultureProvider : IUserCultureProvider
	{
		public CultureInfo Culture { get; private set; }

		public TestCultureProvider(CultureInfo cultureInfo)
		{
			Culture = cultureInfo;
		}
	}
}