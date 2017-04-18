using System;
using System.Windows.Controls;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{

    [TestFixture]
    public class VisualTreeTimeZoneInfoTest
    {

        private CrossThreadTestRunner testRunner;

        [SetUp]
        public void Setup()
        {
            testRunner = new CrossThreadTestRunner();
        }
        [Test]
        public void VerifyTranslatePropertyIsSetsAndInherits()
        {
            testRunner.RunInSTA(
                delegate
                {
                    StackPanel stackPanel = new StackPanel();
                    Assert.AreEqual(TimeZoneInfo.Local,VisualTreeTimeZoneInfo.GetTimeZoneInfo(stackPanel)); //Default should be TimeZoneInfo.Local
                    TextBlock tb = new TextBlock();
                    stackPanel.Children.Add(tb);
                    VisualTreeTimeZoneInfo.SetTimeZoneInfo(stackPanel, TimeZoneInfo.Utc);
                    Assert.AreEqual(TimeZoneInfo.Utc,VisualTreeTimeZoneInfo.GetTimeZoneInfo(tb)); //props are set and inherited
                });
        }

        [Test]
        public void VerifyDataContextGetsCalledIfImplementsIVisualTimeZoneInfoMonitor()
        {
            testRunner.RunInSTA(
                delegate
                {
                    TestClass testClass = new TestClass();
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.DataContext = testClass;

                    Assert.AreNotEqual(testClass.CurrentTimeZone, TimeZoneInfo.Utc); 
                    VisualTreeTimeZoneInfo.SetTimeZoneInfo(stackPanel, TimeZoneInfo.Utc);
                    Assert.AreEqual(TimeZoneInfo.Utc, testClass.CurrentTimeZone,"The timezone has been set on the datacontext"); 
                });
        }

        private class TestClass : IVisualTimeZoneInfoMonitor
        {
            public TimeZoneInfo CurrentTimeZone { get; private set; }
            
            public void TimeZoneInfoChanged(TimeZoneInfo newTimeZoneInfo)
            {
                CurrentTimeZone = newTimeZoneInfo;    
            }
        }
    }
}
