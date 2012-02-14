#region Imports
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
#endregion

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests EmployeeOptionalColumnMapper
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 8/13/2008
    /// </remarks>
    [TestFixture]
    public class EmployeeOptionalColumnMapperTest
    {
        #region Fields - Instance Member

        private EmployeeOptionalColumn _oldEntity;
        private IOptionalColumn _newEntity;
        private EmployeeOptionalColumnMapper _target;

        #endregion

        #region Methods - Instance Member
        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            //Setup _oldEntity
            _oldEntity = new EmployeeOptionalColumn(8, "testColumn");

            //Setup _newEntity
            _newEntity = new OptionalColumn("testColumn");
            _newEntity.TableName = "Person";
            OptionalColumnValue value = new OptionalColumnValue("TestValue");
            _newEntity.AddOptionalColumnValue(value);

            //Setup _target
            ICccListCollection<AgentProperty> propertyColection = new CccListCollection<AgentProperty>();
            AgentProperty agentProperty = new AgentProperty(_oldEntity, "TestValue");
            propertyColection.Add(agentProperty);

            Agent agent = new Agent(-1, "", "", "", "", null, null, propertyColection, "");
            IPerson person = new Person();

            ICollection<Agent> agentCollection = new List<Agent>();
            agentCollection.Add(agent);
          
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.OptionalColumn = new ObjectPairCollection<EmployeeOptionalColumn, IOptionalColumn>();
            mappedObjectPair.Agent = new ObjectPairCollection<Agent, IPerson>();
            mappedObjectPair.Agent.Add(agent, person);

            _target = new EmployeeOptionalColumnMapper(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc), agentCollection);
        }

        #region Test Methods

        /// <summary>
        /// Verifies the column name mapped.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        [Test]
        public void VerifyColumnNameMapped()
        {
            IOptionalColumn optionalColumn = _target.Map(_oldEntity);
            string actualValue = _newEntity.Name;
            string expectedValue = optionalColumn.Name;

            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the column values mapped.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        [Test]
        public void VerifyColumnValuesMapped()
        {
            IOptionalColumn optionalColumn = _target.Map(_oldEntity);
            int actualValue = _newEntity.ValueCollection.Count;
            int expectedValue = optionalColumn.ValueCollection.Count;

            Assert.AreEqual(expectedValue, actualValue);
        }

        #endregion

        #endregion
    }
}
