using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for RepositoryServiceWithUnitOfWork
    /// </summary>
    [TestFixture]
    public class RepositoryServiceWithUnitOfWorkTest 
    {
        private MockRepository _mocks;
        private RepositoryServiceWithUnitOfWorkTestClass _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new RepositoryServiceWithUnitOfWorkTestClass();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            IRepositoryFactory repFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new RepositoryServiceWithUnitOfWorkTestClass(repFactory, unitOfWorkFactory);

            Assert.IsNotNull(_target);
            Assert.AreEqual(repFactory, _target.RepositoryFactory);
            Assert.AreEqual(unitOfWorkFactory, _target.UnitOfWorkFactory);
        }

        [Test]
        public void VerifyRepositoryFactoryProperty()
        {
            IRepositoryFactory repFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target.SetRepositoryFactory(repFactory);
            Assert.AreEqual(repFactory, _target.RepositoryFactory);
        }

        [Test]
        public void VerifyUnitOfWorkFactoryProperty()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target.SetUnitOfWorkFactory(unitOfWorkFactory);
            Assert.AreEqual(unitOfWorkFactory, _target.UnitOfWorkFactory);
        }

    }
}