using System;
using System.Data;
using System.Globalization;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class PersonRotationMapperTest : MapperTest<DataRow>
    {
        private MappedObjectPair mappedObjectPair;
        ObjectPairCollection<DataRow, IPersonRotation> _personRotations;
        private PersonRotationMapper personRotationMapper;
        private DataRow oldRow;
        private IPerson person;
        private Rotation rotation;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }


        private void SetupObjectPairs()
        {
            mappedObjectPair = new MappedObjectPair();
            _personRotations = new ObjectPairCollection<DataRow, IPersonRotation>();
            ObjectPairCollection<global::Domain.Agent, IPerson> _emps = new ObjectPairCollection<Agent, IPerson>();

            Agent agent = new Agent(1,"ffd","ff","sdfdf","dlf",null,null,null,"");
            person = new Domain.Common.Person();
            _emps.Add(agent, person);

            ObjectPairCollection<DataRow, IRotation> _rotation = new ObjectPairCollection<DataRow, IRotation>();

            DataTable t = new DataTable("table1");
            t.Locale = CultureInfo.CurrentCulture;
            t.Columns.Add("rotation_id", typeof(int));
            t.Columns.Add("rotation_name", typeof(string));
            t.Columns.Add("weeks", typeof(int));
            DataRow rotRow = t.NewRow();
            rotRow["rotation_id"] = 10;
            rotRow["rotation_name"] = "Ola";
            rotRow["weeks"] = 2;

            rotation = new Rotation("asfdsf", 27);
            _rotation.Add(rotRow, rotation);

            mappedObjectPair.PersonRotations = _personRotations;
            mappedObjectPair.Agent = _emps;
            mappedObjectPair.Rotations = _rotation;

        }
        
        [SetUp]
        public void Setup()
        {
            SetupObjectPairs();

            DataTable t3 = new DataTable("table2");
            t3.Locale = CultureInfo.CurrentCulture;
            t3.Columns.Add("rotation_id", typeof(int));
            t3.Columns.Add("emp_id", typeof(int));
            t3.Columns.Add("start", typeof(int));
            t3.Columns.Add("date_from", typeof(DateTime));
            
            oldRow = t3.NewRow();

            oldRow["rotation_id"] = 10;
            oldRow["emp_id"] = 1;
            oldRow["date_from"] = new DateTime(2008, 1, 1, 0, 0, 0);
            oldRow["start"] = 2;
            
            personRotationMapper = new PersonRotationMapper(mappedObjectPair);
        }

        [Test]
        public void VerifyGetObjectPairCollection()
        {
            Assert.AreSame(_personRotations, mappedObjectPair.PersonRotations);
        }

        [Test]
        public void VerifyConversion()
        {
            IPersonRotation newPersRot = personRotationMapper.Map(oldRow);
            Assert.AreEqual(7, newPersRot.StartDay);
            Assert.AreEqual(new DateOnly(2008, 1, 1), newPersRot.StartDate);
            Assert.AreSame(person, newPersRot.Person);
            Assert.AreSame(rotation, newPersRot.Rotation);

        }
    }
}
