using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    [TestFixture]
    public class VisualTreeFinderTest
    {
        private VisualTreeFinder _target;
        private Grid _outerGrid;
        private Grid _innerGrid;
        private Button _outerButton;
        private Button _innerButton;
        private TextBlock _textBlock;
        private CrossThreadTestRunner _testRunner;


        [SetUp]
        public void Setup()
        {
            _testRunner = new CrossThreadTestRunner();
            _target = new VisualTreeFinder();

        }

        [Test]
        public void VerifyGetChildElementByType()
        {
            _testRunner.RunInSTA(
                delegate
                {
                    _outerGrid = new Grid();
                    _innerGrid = new Grid();
                    _outerButton = new Button();
                    _innerButton = new Button();
                    _textBlock = new TextBlock();
                    _outerGrid.Children.Add(_innerGrid); //part Child
                    _innerGrid.Children.Add(_outerButton);
                    _outerButton.Content = _innerButton; //part Content
                    _innerButton.Content = _textBlock;

                    Assert.AreEqual(_outerButton, _target.GetDescendantByType(_outerGrid, typeof(Button)), "Verify that we get the first element from the outside");
                    Assert.AreEqual(_textBlock, _target.GetDescendantByType(_innerGrid, typeof(TextBlock)), "Verify that it works on contentcontrols as well");
                    Assert.IsNull(_target.GetDescendantByType(_outerGrid, typeof(Expander)), "verify returns null if type of element does not exist in the visualtree");
                });
        }

        [Test]
        public void VerifyGetChildByTypeAndHook()
        {
            _testRunner.RunInSTA(
               delegate
               {

                   string inner = "inner";

                   _outerGrid = new Grid();
                   _innerGrid = new Grid();
                   _outerButton = new Button();
                   _innerButton = new Button();
                   _innerButton.Name = inner;
                   _textBlock = new TextBlock();
                   _outerGrid.Children.Add(_innerGrid); //part Child
                   _innerGrid.Children.Add(_outerButton);
                   _outerButton.Content = _innerButton; //part Content
                   _innerButton.Content = _textBlock;

                   Assert.AreNotEqual(_innerButton, _target.GetDescendantByType(_outerGrid, typeof(Button)), " will get the outerbutton");
                   NameFinder finder = new NameFinder(inner);
                   Assert.IsFalse(finder.IsSatisfiedBy(_outerButton));
                   Assert.IsTrue(finder.IsSatisfiedBy(_innerButton));
                   _target = new VisualTreeFinder(finder);
                   Assert.AreEqual(_innerButton, _target.GetDescendantByType(_outerGrid, typeof(Button)), "will get the innerbutton since the outer isnt satisfied");
                 
               });
        }


        //Testclass for testing the hook
        private class NameFinder : Specification<Visual>
        {
            private string _name;

            public NameFinder(string name)
            {
                _name = name;
            }
            public override bool IsSatisfiedBy(Visual obj)
            {
                FrameworkElement fe = obj as FrameworkElement;
                return fe != null && fe.Name == _name;
            }
        }

        //Note: Not able to test, need to load the visuals first, lot of wok for a little test.
        //[Test]
        //public void VerifyGetAncestorByType()
        //{

        //    Assert.AreEqual(_innerButton, _target.GetAncestorByType(_textBlock, typeof(Button)));
        //    Assert.AreEqual(_innerGrid, _target.GetAncestorByType(_textBlock, typeof(Grid)));
        //    Assert.IsNull(_target.GetAncestorByType(_textBlock, typeof(Expander)));

        //}



    }
}
