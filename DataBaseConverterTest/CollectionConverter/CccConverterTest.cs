using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.Domain.Common;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    /// <summary>
    /// Tests for Persister
    /// </summary>
    [TestFixture]
    public class CccConverterTest
    {
        private MockRepository mocks;
        private IUnitOfWork uow;
        private Mapper<IEntity, int> mapper;
        private testPersister target;
        private IRepository<IEntity> rep;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mapper = mocks.StrictMock<Mapper<IEntity, int>>(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Local));
            rep = mocks.StrictMock<IRepository<IEntity>>();
            target = new testPersister(uow, mapper, rep);
        }

        [Test]
        public void VerifyConstructorAndGetters()
        {
            Assert.IsNotNull(target);
            Assert.AreSame(uow, target.UnitOfWork);
            Assert.AreSame(mapper, target.Mapper);
        }

        /// <summary>
        /// Verifies the restrictions are checked.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        [Test]
        public void VerifyRestrictionsAreChecked()
        {
            Mapper<IEntity, int> restrictionMapper = mocks.StrictMock<Mapper<IEntity, int>>(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Local));
            CccConverter<IEntity, int> restrictionTarget = mocks.PartialMock<CccConverter<IEntity, int>>(uow, restrictionMapper);
            IEntity restrictionChecker = mocks.StrictMultiMock<IEntity>(typeof(IRestrictionChecker));
            IList<int> checkerList = new List<int>();
            checkerList.Add(1);

            using (mocks.Record())
            {
                restrictionTarget.ConvertAndPersist(checkerList);
                LastCall.CallOriginalMethod(OriginalCallOptions.NoExpectation);
                Expect.On(restrictionMapper)
                    .Call(restrictionMapper.Map(1))
                    .Return(restrictionChecker);
                ((IRestrictionChecker)restrictionChecker).CheckRestrictions();
                LastCall.Throw(new ValidationException());
                Expect.Call(uow.PersistAll()).Return(null);
            }

            using (mocks.Playback())
            {
                restrictionTarget.ConvertAndPersist(checkerList);
            }
        }

        [Test]
        public void VerifyConvertAndPersist()
        {
            IList<int> intList = new List<int>();
            intList.Add(1);
            intList.Add(2);

            IEntity entity1 = mocks.StrictMock<IEntity>();
            IEntity entity2 = mocks.StrictMock<IEntity>();

            using (mocks.Record())
            {
                Expect.On(mapper)
                    .Call(mapper.Map(1))
                    .Return(entity1);
                Expect.On(mapper)
                    .Call(mapper.Map(2))
                    .Return(entity2);
                Expect.Call(entity1.Id).Return(null);
                rep.Add(entity1);
                Expect.Call(entity2.Id).Return(null);
                rep.Add(entity2);
                Expect.Call(uow.PersistAll()).Return(null);
            }
            using (mocks.Playback())
            {
                target.ConvertAndPersist(intList);
            }
            Assert.AreEqual(entity1, target.objectPairCollection.GetPaired(1));
            Assert.AreEqual(entity2, target.objectPairCollection.GetPaired(2));
        }

        private class testPersister : CccConverter<IEntity, int>
        {
            internal ObjectPairCollection<int, IEntity> objectPairCollection;
            private readonly IRepository<IEntity> _rep;

            public testPersister(IUnitOfWork unitOfWork,
                                 Mapper<IEntity, int> mapper,
                                 IRepository<IEntity> rep)
                : base(unitOfWork, mapper)
            {
                _rep = rep;
            }

            public override IRepository<IEntity> Repository
            {
                get { return _rep; }
            }

            protected override void OnPersisted(ObjectPairCollection<int, IEntity> pairCollection)
            {
                objectPairCollection = pairCollection;
            }

        }

    }
}