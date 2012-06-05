using System;
using System.Collections.Generic;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Person=Teleopti.Ccc.Domain.Common.Person;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{

    [TestFixture]
    public class EmployeeOptionalColumnMapperTest
    {
        private EmployeeOptionalColumn _oldEntity;
        private IOptionalColumn _newEntity;
        private EmployeeOptionalColumnMapper _target;
    	private Agent _agent;

    	[SetUp]
        public void Setup()
        {
            //Setup _oldEntity
            _oldEntity = new EmployeeOptionalColumn(8, "testColumn");

            //Setup _newEntity
        	_newEntity = new OptionalColumn("testColumn") {TableName = "Person"};
        	
            //Setup _target
            ICccListCollection<AgentProperty> propertyColection = new CccListCollection<AgentProperty>();
            var agentProperty = new AgentProperty(_oldEntity, "TestValue");
            propertyColection.Add(agentProperty);

            _agent = new Agent(-1, "", "", "", "", null, null, propertyColection, "");
            IPerson person = new Person();
			
            ICollection<Agent> agentCollection = new List<Agent>();
            agentCollection.Add(_agent);

    		var mappedObjectPair = new MappedObjectPair
    		                       	{
    		                       		OptionalColumn =
    		                       			new ObjectPairCollection<EmployeeOptionalColumn, IOptionalColumn>(),
    		                       		Agent = new ObjectPairCollection<Agent, IPerson>{{_agent, person}}
    		                       	};
			
    		_target = new EmployeeOptionalColumnMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc), agentCollection);
        }

    	[Test]
        public void VerifyColumnNameMapped()
        {
            IOptionalColumn optionalColumn = _target.Map(_oldEntity);
            string actualValue = _newEntity.Name;
            string expectedValue = optionalColumn.Name;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyColumnValuesMapped()
        {
			_target.Map(_oldEntity);
			IPerson person = _target.MappedObjectPair.Agent.GetPaired(_agent);

			int actualValue = person.OptionalColumnValueCollection.Count;
			int expectedValue = _agent.PropertyCollection.Count;

            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
