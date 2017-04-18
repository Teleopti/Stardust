using System.Windows;
using System.Windows.Controls;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    [TestFixture]
    public class VisualTreeTranslatorTest
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
                        Assert.IsFalse(VisualTreeTranslator.GetTranslate(stackPanel)); //Default should be false
                        TextBlock tb = new TextBlock();
                        stackPanel.Children.Add(tb);
                        VisualTreeTranslator.SetTranslate(stackPanel, true);
                        Assert.IsTrue(VisualTreeTranslator.GetTranslate(tb)); //props are set and inherited
                    });
        }
        [Test]
        public void VerifyTranslateIsCalledWhenLoaded()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        string xxOK = "xxOK";
                        string OK = "OK";
                        TestClass translateTarget = new TestClass() { Text = xxOK };
                        VisualTreeTranslator.SetTranslate(translateTarget, true);
                        translateTarget.TriggerLoad();
                        Assert.AreEqual(OK, translateTarget.Text); //Load has triggered textsetter 
                        translateTarget.Text = xxOK;
                        translateTarget.TriggerLoad();
                        Assert.AreEqual(xxOK, translateTarget.Text); //Load event is removed, only translates one time

                    });
        }

        [Test]
        public void VerifyTranslateReturnsSameStringIfNotTranslated()
        {
            testRunner.RunInSTA(
                delegate
                {
                    string notTranslatableText = "xxthisStringStartsWithxxButWillNotBeTranslated";
                    TestClass translateTarget = new TestClass() { Text = notTranslatableText };
                    VisualTreeTranslator.SetTranslate(translateTarget, true);
                    translateTarget.TriggerLoad();
                    Assert.AreEqual(notTranslatableText, translateTarget.Text); //Load has triggered textsetter 

                });
        }

        //need a derived TextBlock to be able to call load
        private class TestClass : TextBlock
        {
          internal void TriggerLoad()
          {
              RaiseEvent(new RoutedEventArgs(LoadedEvent));
          }
        }
    }
}
