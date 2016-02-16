using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Tests for the AbstractPropertyPages class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    [TestFixture]
    public class AbstractPropertyPagesTest
    {
        private MockRepository mocks;
        private AbstractPropertyPages<IAggregateRoot> target;
        private IList<IPropertyPage> pages;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepositoryFactory repositoryFactory;
        private ILazyLoadingManager lazyManager;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            IPropertyPage propertyPage1 = mocks.StrictMock<IPropertyPage>();
            IPropertyPage propertyPage2 = mocks.StrictMock<IPropertyPage>();
            IPropertyPage propertyPage3 = mocks.StrictMock<IPropertyPage>();
            pages = new List<IPropertyPage>
                        {
                propertyPage1,
                propertyPage2,
                propertyPage3
            };
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();

            target = mocks.Stub<AbstractPropertyPages<IAggregateRoot>>(repositoryFactory,unitOfWorkFactory);
        	lazyManager = mocks.DynamicMock<ILazyLoadingManager>();
            target.Initialize(pages, lazyManager);
        }

        /// <summary>
        /// Verfiys the create.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyCreate()
        {
            mocks.ReplayAll();

            Assert.IsNotNull(target);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the default properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
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

        /// <summary>
        /// Verifies the aggregate root object works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyAggregateRootObjectWorks()
        {
            IAggregateRoot rootEntity = mocks.StrictMock<IAggregateRoot>();
            Expect.On(target).Call(target.CreateNewRoot()).Return(rootEntity);

            mocks.ReplayAll();

            Assert.AreEqual(rootEntity, target.AggregateRootObject);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the can find my next page works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyCanFindMyNextPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[1], target.FindMyNextPage(pages[0]));
            Assert.AreEqual(pages[2], target.FindMyNextPage(pages[1]));
            Assert.AreEqual(pages[2], target.FindMyNextPage(pages[2]));
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the can find my previous page works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyCanFindMyPreviousPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.FindMyPreviousPage(pages[0]));
            Assert.AreEqual(pages[0], target.FindMyPreviousPage(pages[1]));
            Assert.AreEqual(pages[1], target.FindMyPreviousPage(pages[2]));
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the current page works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyCurrentPageWorks()
        {
            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            target.CurrentPage = pages[1];
            Assert.AreEqual(pages[1], target.CurrentPage);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the overloaded constructor works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifyOverloadedConstructorWorks()
        {
            IAggregateRoot rootEntity = mocks.StrictMock<IAggregateRoot>();
            target = mocks.StrictMock<AbstractPropertyPages<IAggregateRoot>>(rootEntity,repositoryFactory,unitOfWorkFactory);

            mocks.ReplayAll();

            Assert.AreEqual(rootEntity, target.AggregateRootObject);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the set owner.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        public void VerifySetOwner()
        {
            mocks.ReplayAll();

            Form emptyForm = new Form();
            target.Owner = emptyForm;
            Assert.AreEqual(emptyForm, target.Owner);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the is on last works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
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

        /// <summary>
        /// Verifies the is on first works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
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

        /// <summary>
        /// Verifies the next page works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyNextPageWorks()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();

            Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();

            Expect.Call(pages[0].Depopulate(aggregateRoot)).Return(true).Repeat.Once();
            pages[1].Populate(aggregateRoot);

            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            Assert.AreEqual(pages[1], target.NextPage());
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the dispose works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyDisposeWorks()
        {
            target.Dispose();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            foreach (IPropertyPage page in pages)
            {
                page.Dispose();
            }

            mocks.ReplayAll();

            target.Dispose();
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the dispose works with owner.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyDisposeWorksWithOwner()
        {
            Form emptyForm = new Form();
            target.Owner = emptyForm;
            target.Dispose();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            foreach (IPropertyPage page in pages)
            {
                page.Dispose();
            }
            
            mocks.ReplayAll();

            target.Dispose();
            Assert.IsTrue(target.Owner.IsDisposed);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the previous page works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyPreviousPageWorks()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();

            Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();

            Expect.Call(pages[1].Depopulate(aggregateRoot)).Return(true).Repeat.Once();
            pages[0].Populate(aggregateRoot);

            mocks.ReplayAll();

            target.CurrentPage = pages[1];
            Assert.AreEqual(pages[1], target.CurrentPage);
            Assert.AreEqual(pages[0], target.PreviousPage());
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the page names.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyPageNames()
        {
            Expect.On(pages[0]).Call(pages[0].PageName).Return("page0").Repeat.Once();
            Expect.On(pages[1]).Call(pages[1].PageName).Return("page1").Repeat.Once();
            Expect.On(pages[2]).Call(pages[2].PageName).Return("page2").Repeat.Once();

            mocks.ReplayAll();

            string[] pageNames = target.GetPageNames();

            Assert.AreEqual("page0", pageNames[0]);
            Assert.AreEqual("page1", pageNames[1]);
            Assert.AreEqual("page2", pageNames[2]);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the save.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifySave()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();
            IUnitOfWork uow = mocks.DynamicMock<IUnitOfWork>();
            Guid newGuid = Guid.NewGuid();

            using(mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Twice();
                Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();
                Expect.Call(aggregateRoot.Id).Return(newGuid).Repeat.Once();

                uow.PersistAll();
                LastCall.Repeat.Twice().Return(
                    new List<IRootChangeInfo>
                        {
                            new RootChangeInfo(aggregateRoot, DomainUpdateType.Update)
                        });

                target.Save();
                LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

                pages[0].Depopulate(null);
                LastCall.IgnoreArguments().Return(true).Repeat.Once();
            }

            using(mocks.Playback())
            {
                IList<IRootChangeInfo> result = new List<IRootChangeInfo>(target.Save());

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(newGuid, ((IAggregateRoot)result[0].Root).Id);
                Assert.IsInstanceOf<IAggregateRoot>(result[0].Root);
            }
        }

        /// <summary>
        /// Verifies the save with validation error from page.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        public void VerifySaveWithValidationErrorFromPage()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();
            Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();

            target.Save();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            pages[0].Depopulate(null);
            LastCall.IgnoreArguments().Return(false).Repeat.Once();

            mocks.ReplayAll();

            Assert.IsNull(target.Save());
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the show page.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyShowPage()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();
            Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();

            pages[0].Depopulate(null);
            LastCall.IgnoreArguments().Return(true).Repeat.Once();

            pages[2].Populate(null);
            LastCall.IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(pages[0], target.CurrentPage);
            Assert.AreEqual(pages[2], target.ShowPage(pages[2]));
            Assert.AreEqual(pages[2], target.CurrentPage);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the name changed event.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyNameChangedEvent()
        {
            INameChangedEventTester nameChangedEventTester = mocks.StrictMock<INameChangedEventTester>();
            target.NameChanged += nameChangedEventTester.NameHasChanged;

            nameChangedEventTester.NameHasChanged(null, null);
            LastCall.IgnoreArguments()
                .Repeat.Once();

            mocks.ReplayAll();

            target.TriggerNameChanged(new WizardNameChangedEventArgs("myNewName"));
            mocks.VerifyAll();
        }


        /// <summary>
        /// Verifies the load aggregate root working copy.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-25
        /// </remarks>
        [Test]
        public void VerifyLoadAggregateRootWorkingCopy()
        {
            IAggregateRoot rootEntity = mocks.StrictMock<IAggregateRoot>();
            
            target = mocks.StrictMock<AbstractPropertyPages<IAggregateRoot>>(rootEntity, repositoryFactory,unitOfWorkFactory);
			target.Initialize(new List<IPropertyPage>(), lazyManager);
            IRepository<IAggregateRoot> aggregateRepository = mocks.StrictMock<IRepository<IAggregateRoot>>();

            Guid? newGuid = Guid.NewGuid();
            Expect.Call(rootEntity.Id).Return(newGuid).Repeat.Twice();
            Expect.Call(target.RepositoryObject).Return(aggregateRepository).Repeat.Once();
            Expect.Call(aggregateRepository.Get(newGuid.Value)).Return(rootEntity).Repeat.Once();

            target.LoadAggregateRootWorkingCopy();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            mocks.ReplayAll();

            target.LoadAggregateRootWorkingCopy();
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the add to repository gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void VerifyAddToRepositoryGivesException()
        {
            target.AddToRepository();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

            mocks.ReplayAll();

            target.AddToRepository();
            mocks.VerifyAll();
        }
    }

    /// <summary>
    /// Interface for mocking event
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-17
    /// </remarks>
    public interface INameChangedEventTester
    {
        /// <summary>
        /// The name has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard.WizardNameChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        void NameHasChanged(object sender, WizardNameChangedEventArgs e);
    }
}
