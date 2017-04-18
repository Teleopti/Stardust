using System.Threading;
using System.Windows;
using System.Windows.Controls;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.CustomPanels.Grouping;

namespace Teleopti.Ccc.WinCodeTest.Common.CustomPanelsTest.Grouping
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class BaseGroupingPanelTest
    {
        private TestPanel _target;
        private ParameterChecker _checker;

        [SetUp]
        public void Setup()
        {
            _checker = new ParameterChecker();
            _target = new TestPanel(_checker);
        }

        [Test]
        public void VerifyCallsDerivedClassOnArrange()
        {
            Size finalSize = new Size(10,20);
            UIElementCollection ui = new UIElementCollection(_target,_target);
            TesterForPanels.CallArrange(_target, finalSize);

            _checker.Verify(finalSize, ui);
        }

        [Test]
        public void VerifyCallsDerivedClassOnMeasure()
        {
            Size availableSize = new Size(10, 20);
            UIElementCollection ui = new UIElementCollection(_target, _target);
            TesterForPanels.CallMeasure(_target, availableSize);

            _checker.Verify(availableSize, ui);

        }

    }

    internal class TestPanel : BaseGroupingPanel
    {
        private readonly ParameterChecker _parameterChecker;

        public TestPanel(ParameterChecker parameterChecker)
        {
            _parameterChecker = parameterChecker;
        }

        protected  override Size MeasureElements(Size availableSize, UIElementCollection elements)
        {
            _parameterChecker.CalledWithElements = elements;
            _parameterChecker.CalledWithSize = availableSize;
            return availableSize; //just to return something
        }

        protected override Size ArrangeElements(Size finalSize, UIElementCollection elements)
        {
            _parameterChecker.CalledWithElements = elements;
            _parameterChecker.CalledWithSize = finalSize;
            return finalSize; //just to return something
        }

     
    }

    internal class ParameterChecker
    {
        public ParameterChecker()
        {
            //Some "non probably" data!
            CalledWithSize = new Size(3215,2);
            CalledWithElements = null;
        }
        internal Size CalledWithSize { get; set; }
        internal UIElementCollection CalledWithElements { get; set; }

        internal void Verify(Size expectedSize,UIElementCollection expectedElementCollection)
        {
            Assert.AreEqual(expectedSize,CalledWithSize);
            Assert.AreEqual(expectedElementCollection,CalledWithElements);
        }
    }
}