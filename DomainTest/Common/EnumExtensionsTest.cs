using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class EnumExtensionsTest
	{
		[Test]
		public void ShouldGetAllValuesWithoutExcepteds()
		{
			var values = EnumExtensions.GetValues(SchedulePeriodType.ChineseMonth);
			Assert.IsFalse(values.Contains(SchedulePeriodType.ChineseMonth));
		}


	}

	

}
