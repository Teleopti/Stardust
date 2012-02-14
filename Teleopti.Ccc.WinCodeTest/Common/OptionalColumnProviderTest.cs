﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class OptionalColumnProviderTest: IDisposable
    {
        private OptionalColumnProvider _target;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _target = new OptionalColumnProvider(_repositoryFactory);
        }

        [Test]
        public void ShouldLoadAll()
        {
            IOptionalColumnRepository optionalColumnRepository = _mocks.StrictMock<IOptionalColumnRepository>();
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();

            using (_mocks.Record())
            {
                Expect.Call(_repositoryFactory.CreateOptionalColumnRepository(unitOfWork)).Return(optionalColumnRepository);
                Expect.Call(optionalColumnRepository.GetOptionalColumnValues<Person>()).Return(new List<IOptionalColumn>() { null });
            }
            using (_mocks.Playback())
            {
                _target.LoadAllOptionalColumns(unitOfWork);
                Assert.AreEqual(1, _target.OptionalColumnCollection.Count());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _target.Dispose();
                _repositoryFactory = null;
            }
        }
    }
}
