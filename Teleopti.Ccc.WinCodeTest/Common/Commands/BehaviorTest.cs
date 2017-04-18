using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands.CommandBehaviors;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{

    [TestFixture]
    public class BehaviorTest
    {

        private CrossThreadTestRunner _testRunner;

        [SetUp]
        public void Setup()
        {
            _testRunner = new CrossThreadTestRunner();
        }

        [Test]
        public void VerifyExpanderExpandedCommand()
        {
            _testRunner.RunInSTA(
              delegate
              {
                  Expander expander = new Expander();
                  ExpanderBehavior.SetExpanderExpandedCommand(expander, ApplicationCommands.NotACommand);
                  Assert.AreEqual(ApplicationCommands.NotACommand, ExpanderBehavior.GetExpanderExpandedCommand(expander));
              });
        }
        [Test]
        public void VerifyUIElementMouseDownCommand()
        {
            _testRunner.RunInSTA(
              delegate
              {
                  TabItem tabItem = new TabItem();
                  UIElementBehavior.SetUIElementMouseDownCommand(tabItem, ApplicationCommands.NotACommand);
                  Assert.AreEqual(ApplicationCommands.NotACommand, UIElementBehavior.GetUIElementMouseDownCommand(tabItem));
              });
        }
    }
}
