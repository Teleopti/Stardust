using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Tests for the AbstractWizardPagesTest class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-17
    /// </remarks>
    [TestFixture]
    public class AbstractWizardPagesTest
    {
        private MockRepository mocks;
        private AbstractWizardPages<IAggregateRoot> target;
        private IList<IPropertyPage> pages;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepositoryFactory repositoryFactory;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
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

            target = mocks.Stub<AbstractWizardPages<IAggregateRoot>>(repositoryFactory, unitOfWorkFactory);
            target.Initialize(pages, new LazyLoadingManagerWrapper());
        }

        /// <summary>
        /// Verifies the can create.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCanCreate()
        {
            Expect.Call(target.ModeCreateNew).CallOriginalMethod(OriginalCallOptions.CreateExpectation).Repeat.Once();

            mocks.ReplayAll();

            Assert.IsTrue(target.ModeCreateNew);
            Assert.IsNotNull(target);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the can create with argument.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCanCreateWithArgument()
        {
            IAggregateRoot rootEntity = mocks.StrictMock<IAggregateRoot>();
            target = mocks.StrictMock<AbstractWizardPages<IAggregateRoot>>(rootEntity,repositoryFactory,unitOfWorkFactory);

            mocks.ReplayAll();
            Assert.IsNotNull(target);
            Assert.AreEqual(rootEntity, target.AggregateRootObject);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the can get repository.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyCanGetRepository()
        {
            IRepository<IAggregateRoot> repository = mocks.StrictMock<IRepository<IAggregateRoot>>();
            Expect.On(target).Call(target.RepositoryObject).Return(repository).Repeat.Once();

            mocks.ReplayAll();

            Assert.AreEqual(repository, target.RepositoryObject);
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the save works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifySaveWorks()
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

                target.GetType()
                    .GetMethod("OnAfterSave",
                               BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                               BindingFlags.InvokeMethod)
                    .Invoke(target, new object[] {});
                LastCall.Repeat.Once();

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
        /// Verifies the add to repository works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-17
        /// </remarks>
        [Test]
        public void VerifyAddToRepositoryWorks()
        {
            IAggregateRoot rootEntity = mocks.StrictMock<IAggregateRoot>();
            target = mocks.StrictMock<AbstractWizardPages<IAggregateRoot>>(rootEntity,repositoryFactory,unitOfWorkFactory);

            target.AddToRepository();
            LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation)
                .Repeat.Once();

            IRepository<IAggregateRoot> repository = mocks.StrictMock<IRepository<IAggregateRoot>>();
            Expect.On(target).Call(target.RepositoryObject).Return(repository).Repeat.Once();

            repository.Add(rootEntity);
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            target.AddToRepository();
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDisposeWorksWithUnitOfWork()
        {
            IAggregateRoot aggregateRoot = mocks.StrictMock<IAggregateRoot>();
            IUnitOfWork uow = mocks.StrictMock<IUnitOfWork>();
            
            using(mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Twice();
                Expect.Call(target.CreateNewRoot()).Return(aggregateRoot).Repeat.Once();

                target.Save();
                LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

                pages[0].Depopulate(null);
                LastCall.IgnoreArguments().Return(true).Repeat.Once();

                uow.PersistAll();
                LastCall.Repeat.Twice().Return(new List<IRootChangeInfo>());

                target.GetType()
                    .GetMethod("OnAfterSave",
                               BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                               BindingFlags.InvokeMethod)
                    .Invoke(target, new object[] {});
                LastCall.Repeat.Once();

                target.Dispose();
                LastCall.CallOriginalMethod(OriginalCallOptions.CreateExpectation);

                uow.Dispose();
                LastCall.Repeat.Twice();

                foreach (IPropertyPage page in pages)
                {
                    page.Dispose();
                }
            }

            using(mocks.Playback())
            {
                //Must call Save to initialize UnitOfWork
                target.Save();
                target.Dispose();
            }
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
    }
}
