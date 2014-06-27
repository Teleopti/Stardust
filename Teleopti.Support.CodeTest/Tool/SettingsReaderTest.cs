using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class SettingsReaderTest
    {
        [Test]
        public void GetSearchReplaceListTest()
        {
            var theString =
                @"$(ConnectionStringApp)|Data Source=.;Initial Catalog=TeleoptiApp_Demo;Integrated Security=True
$(ConnectionStringAnalytics)|Data Source=.;Initial Catalog=TeleoptiAnalytics_Demo;Integrated Security=True";

            var target = new SettingsReader();
            var ret = target.GetSearchReplaceList(theString);
            ret.Count.Should().Be(2);
        }
    }
}
