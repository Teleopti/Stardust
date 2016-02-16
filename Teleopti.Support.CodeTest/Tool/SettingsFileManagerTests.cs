using NUnit.Framework;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Tool.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class SettingsFileManagerTests
    {
        [Test()]
        public void GetReplaceListTest()
        {
            var manager = new SettingsFileManager(new SettingsReader());
            Assert.That(manager.GetReplaceList().Count,Is.GreaterThan(0));
        }

        [Test()]
        public void SaveReplaceListTest()
        {
            var manager = new SettingsFileManager(new SettingsReader());
            var replaceList = manager.GetReplaceList();
            var oldvalue = replaceList[0].ReplaceWith;
            replaceList[0].ReplaceWith = oldvalue + oldvalue;
            manager.SaveReplaceList(replaceList);
            var newReplaceList = manager.GetReplaceList();
            Assert.That(replaceList.Count,Is.EqualTo(newReplaceList.Count));
            Assert.That(newReplaceList[0].ReplaceWith, Is.Not.EqualTo(oldvalue));
            //saveback
            replaceList[0].ReplaceWith = oldvalue;
            manager.SaveReplaceList(replaceList);
        }
    }
}
