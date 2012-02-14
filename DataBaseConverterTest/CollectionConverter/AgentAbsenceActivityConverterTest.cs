using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class AgentAbsenceActivityConverterTest
    {
        private MockRepository _mocks;
        private IUnitOfWork _uow;
        private AgentAbsenceActivityMapper _mapper;
        private MappedObjectPair _mappedObjectPair = new MappedObjectPair();
        AgentDayFactory _factory = new AgentDayFactory();

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _uow = _mocks.StrictMock<IUnitOfWork>();
            _mappedObjectPair.Absence = _factory.AbsPairList;
            _mappedObjectPair.Agent = _factory.AgentPairList;
            _mappedObjectPair.Scenario = _factory.ScenarioPairList;
            _mappedObjectPair.AbsenceActivity = _factory.AbsenceActivityList;
            _mapper = new AgentAbsenceActivityMapper(_mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
        }
        
        [Test]
        public void VerifyProperties()
        {
            IRepository<IPersonAbsence> rep = _mocks.StrictMock<IRepository<IPersonAbsence>>();
            AgentAbsenceActivityConverter target = new testPersister(_uow, _mapper, rep);
            Assert.IsNotNull(target.Mapper);
            Assert.IsNotNull(target.Repository);
            Assert.IsNotNull(target.UnitOfWork);

            AgentAbsenceActivityConverter target2 = new AgentAbsenceActivityConverter(_uow, _mapper);
            Assert.IsNotNull(target2.Repository);
        }

        [Test]
        public void VerifyConvertAndPersist()
        {
            IList<global::Domain.AgentDay> checkerList = new List<global::Domain.AgentDay>();
            IRepository<IPersonAbsence> rep = _mocks.StrictMock<IRepository<IPersonAbsence>>();
            
            global::Domain.AgentDay agDay1 = _factory.AgentDay();
            agDay1.AgentDayAssignment.SetAssigned(_factory.WorkShift(), _factory.ScheduleType);
            checkerList.Add(agDay1);

            AgentAbsenceActivityConverter target = new testPersister(_uow, _mapper, rep);
            using (_mocks.Record())
            {
                rep.Add(null);
                LastCall.IgnoreArguments();
                Expect.Call(_uow.PersistAll()).Return(null);
            }
            using (_mocks.Playback())
            {
                target.ConvertAndPersist(checkerList);
            }
        }

        private class testPersister : AgentAbsenceActivityConverter
        {
            private readonly IRepository<IPersonAbsence> _rep;

            public testPersister(IUnitOfWork unitOfWork,
                                 Mapper<IList<IPersonAbsence>, AgentDay> mapper,
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
