using System;
using System.Drawing;
using Domain;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Collections.Generic;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class AgentPreferenceConverterTest
    {
        private AgentPreferenceConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IPreferenceDay, AgentDayPreference> mapper;
        private MockRepository mocks;
        IRepository<IPreferenceDay> _rep;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IPreferenceDay, AgentDayPreference>>(mappedObjectPair, (TimeZoneInfo.Local));
            target = new AgentPreferenceConverter(uow, mapper);
            _rep = mocks.StrictMock<IRepository<IPreferenceDay>>();
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
            DateOnly requestDate = new DateOnly(2008, 12, 24);
            PreferenceRestriction restriction = new PreferenceRestriction();
            PreferenceDay personRestriction = new PreferenceDay(person, requestDate, restriction);
            AgentDayPreference agentPreference = new AgentDayPreference(false);
            agentPreference.PreferenceDate = DateTime.SpecifyKind(requestDate.Date,DateTimeKind.Local);
            Agent agent = new Agent(1, "Kalle", "Stropp","","",null,null,null,"");
                    agentPreference.Agent = agent;
            agentPreference.ShiftCategory = new ShiftCategory(1,"test", "t",Color.DodgerBlue,true,true,1);
                    IList<AgentDayPreference> list = new List<AgentDayPreference>();
            list.Add(agentPreference);

            AgentPreferenceConverter target2 = new testPersister(uow, mapper, _rep);
            using (mocks.Record())
            {
                _rep.Add(null);
                LastCall.IgnoreArguments();
                Expect.Call(target2.Mapper.Map(agentPreference)).Return(personRestriction);
                Expect.Call(uow.PersistAll()).Return(null);
            }
            using (mocks.Playback())
            {
                target2.ConvertAndPersist(list);
            }
        }

        private class testPersister : AgentPreferenceConverter
        {
            private readonly IRepository<IPreferenceDay> _rep;

            public testPersister(IUnitOfWork unitOfWork,
                                 Mapper<IPreferenceDay, AgentDayPreference> mapper,
                                 IRepository<IPreferenceDay> rep)
                : base(unitOfWork, mapper)
            {
                _rep = rep;
            }

            public override IRepository<IPreferenceDay> Repository
            {
                get { return _rep; }
            }
        }
    }
}
