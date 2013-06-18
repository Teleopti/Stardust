using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Collections.Generic;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class AgentAbsenceConverterTest
    {
        private readonly AgentDayFactory _factory = new AgentDayFactory();
        private AgentAbsenceConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IPersonAbsence, global::Domain.AgentDay> mapper;
        private MockRepository mocks;
        private IRepository<IPersonAbsence> rep;
        private IDictionary<global::Domain.Agent, IList<global::Domain.AgentDay>> dic;
        private IList<global::Domain.AgentDay> list;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IPersonAbsence, global::Domain.AgentDay>>(mappedObjectPair, (TimeZoneInfo.Local));
            target = new AgentAbsenceConverter(uow, mapper);
            rep = mocks.StrictMock<IRepository<IPersonAbsence>>();
            dic = new Dictionary<global::Domain.Agent, IList<global::Domain.AgentDay>>();
            list = new List<global::Domain.AgentDay>();
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyConvertAndPersist()
        {
            IPerson person = PersonFactory.CreatePerson();
            IAbsence abs = AbsenceFactory.CreateAbsence("abs");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();

            IPersonAbsence personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 1, 2001, 1, 2), abs);
            IPersonAbsence personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2001, 1, 1, 2001, 1, 3), abs);

            global::Domain.AgentDay agDay1 = _factory.AgentDay();
            agDay1.AgentDayAssignment.SetAssigned(_factory.WorkShift(), _factory.ScheduleType);
            list.Add(agDay1);
            list.Add(agDay1);
            dic.Add(agDay1.AssignedAgent, list);

            AgentAbsenceConverter target2 = new testPersister(uow, mapper, rep);
            using (mocks.Record())
            {
                rep.Add(null);
                LastCall.IgnoreArguments();
                Expect.Call(target2.Mapper.Map(agDay1)).Return(personAbsence);
                Expect.Call(target2.Mapper.Map(agDay1)).Return(personAbsence2);
                Expect.Call(uow.PersistAll()).Return(null);
            }
            using (mocks.Playback())
            {
                target2.ConvertAndPersist(dic);
            }
        }

        private class testPersister : AgentAbsenceConverter
        {
            private readonly IRepository<IPersonAbsence> _rep;

            public testPersister(IUnitOfWork unitOfWork,
                                 Mapper<IPersonAbsence, global::Domain.AgentDay> mapper,
                                 IRepository<IPersonAbsence> rep)
                : base(unitOfWork, mapper)
            {
                _rep = rep;
            }

            public override IRepository<IPersonAbsence> Repository
            {
                get { return _rep; }
            }
        }
    }
}
