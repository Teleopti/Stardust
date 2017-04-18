using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.WinCodeTest.Common.PropertyPageAndWizard
{
   
    [TestFixture]
    public class AbstractPropertyPagesNoRootTest
    {
        private MockRepository mocks;
        private AbstractPropertyPagesNoRoot<object> target;
        private IList<IPropertyPageNoRoot<object>> pages;
#pragma warning disable 649
        private object tesobj; 
#pragma warning restore 649

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            var propertyPage1 = mocks.StrictMock<IPropertyPageNoRoot<object>>();
            var propertyPage2 = mocks.StrictMock<IPropertyPageNoRoot<object>>();
            var propertyPage3 = mocks.StrictMock<IPropertyPageNoRoot<object>>();
            pages = new List<IPropertyPageNoRoot<object>>
                        {
                propertyPage1,
                propertyPage2,
                propertyPage3
            };

            target = mocks.Stub<AbstractPropertyPagesNoRoot<object>>(tesobj);
            target.Initialize(pages);
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Expect.Call(target.MinimumSize).CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Twice();
            Expect.Call(target.ModeCreateNew).CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(3, target.Pages.Count);
            Assert.AreEqual(pages[0], target.Pages[0]);
            Assert.AreEqual(pages[0], target.FirstPage);
            Assert.AreEqual(pages[0], target.CurrentPage);
            Assert.IsFalse(target.ModeCreateNew);

            Assert.AreEqual(550, target.MinimumSize.Width);
            Assert.AreEqual(400, target.MinimumSize.Height);

            Assert.IsNull(target.Owner);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyAggregateRootObjectWorks()
        {
            var obj = new object();
            Expect.On(target).Call(target.CreateNewStateObj()).Return(obj);

            mocks.ReplayAll();

            Assert.AreEqual(obj, target.StateObj);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanFindMyNextPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[1], target.FindMyNextPage(pages[0]));
            Assert.AreEqual(pages[2], target.FindMyNextPage(pages[1]));
            Assert.AreEqual(pages[2], target.FindMyNextPage(pages[2]));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanFindMyPreviousPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.FindMyPreviousPage(pages[0]));
            Assert.AreEqual(pages[0], target.FindMyPreviousPage(pages[1]));
            Assert.AreEqual(pages[1], target.FindMyPreviousPage(pages[2]));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCurrentPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            target.CurrentPage = pages[1];
            Assert.AreEqual(pages[1], target.CurrentPage);
            mocks.VerifyAll();
        }

     
        [Test]
        public void VerifyOverloadedConstructorWorks()
        {
            var obj = new object();
            target = mocks.StrictMock<AbstractPropertyPagesNoRoot<object>>(obj);

            mocks.ReplayAll();

            Assert.AreEqual(obj, target.StateObj);
            mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifySetOwner()
        {
            mocks.ReplayAll();

            Form emptyForm = new Form();
            target.Owner = emptyForm;
            Assert.AreEqual(emptyForm, target.Owner);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyIsOnLastWorks()
        {
            mocks.ReplayAll();

            target.CurrentPage = pages[1];
            Assert.IsFalse(target.IsOnLast());
            target.CurrentPage = pages[2];
            Assert.IsTrue(target.IsOnLast());
            target.CurrentPage = pages[0];
            Assert.IsFalse(target.IsOnLast());
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyIsOnFirstWorks()
        {
            mocks.ReplayAll();

            target.CurrentPage = pages[1];
            Assert.IsFalse(target.IsOnFirst());
            target.CurrentPage = pages[0];
            Assert.IsTrue(target.IsOnFirst());
            target.CurrentPage = pages[2];
            Assert.IsFalse(target.IsOnFirst());
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyNextPageWorks()
        {
            var obj = new object();
            Expect.Call(target.CreateNewStateObj()).Return(obj).Repeat.Once();

            Expect.Call(pages[0].Depopulate(obj)).Return(true).Repeat.Once();
            pages[1].Populate(obj);

            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            Assert.AreEqual(pages[1], target.NextPage());
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDisposeWorks()
        {
            target.Dispose();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            foreach (var page in pages)
            {
                page.Dispose();
            }

            mocks.ReplayAll();

            target.Dispose();
            mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifyDisposeWorksWithOwner()
        {
            Form emptyForm = new Form();
            target.Owner = emptyForm;
            target.Dispose();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            foreach (var page in pages)
            {
                page.Dispose();
            }
            
            mocks.ReplayAll();

            target.Dispose();
            Assert.IsTrue(target.Owner.IsDisposed);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyPreviousPageWorks()
        {
            var obj = new object();

            Expect.Call(target.CreateNewStateObj()).Return(obj).Repeat.Once();

            pages[0].Populate(obj);

            mocks.ReplayAll();

            target.CurrentPage = pages[1];
            Assert.AreEqual(pages[1], target.CurrentPage);
            Assert.AreEqual(pages[0], target.PreviousPage());
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyPageNames()
        {
            Expect.On(pages[0]).Call(pages[0].PageName).Return("page0").Repeat.Once();
            Expect.On(pages[1]).Call(pages[1].PageName).Return("page1").Repeat.Once();
            Expect.On(pages[2]).Call(pages[2].PageName).Return("page2").Repeat.Once();

            mocks.ReplayAll();

            var pageNames = target.GetPageNames();

            Assert.AreEqual("page0", pageNames[0]);
            Assert.AreEqual("page1", pageNames[1]);
            Assert.AreEqual("page2", pageNames[2]);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyShowPage()
        {
            var obj = new object();

            Expect.Call(target.CreateNewStateObj()).Return(obj).Repeat.Once();

            pages[0].Depopulate(0);
            LastCall.IgnoreArguments().Return(true).Repeat.Once();

            pages[2].Populate(0);
            LastCall.IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            Assert.AreEqual(pages[2], target.ShowPage(pages[2]));
            Assert.AreEqual(pages[2], target.CurrentPage);
            mocks.VerifyAll();
        }
    }
}
