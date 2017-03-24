using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Util;

namespace Teleopti.Ccc.InfrastructureTest.Util
{
	[TestFixture]
    class StringHelperTest
    {
        [Test]
        public void ShouldDetectChineseInString()
        {
            var inputWithChinese = "小王";
            StringHelper.StringContainsChinese(inputWithChinese).Should().Be.True();

            var inputWithoutChinese = "english";
            StringHelper.StringContainsChinese(inputWithoutChinese).Should().Be.False();

            var inputWithBothChineseAndEnglish = "王小ming";
            StringHelper.StringContainsChinese(inputWithBothChineseAndEnglish).Should().Be.True();
        }
    }
}
