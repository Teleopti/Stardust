using System;
using System.Data;
using System.Globalization;
using Domain;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class PersonAvailabilityMapperTest : MapperTest<DataRow>
    {
        private MappedObjectPair _mappedObjectPair;
        private ObjectPairCollection<DataRow, IPersonAvailability> _personAvailabilities;
        private PersonAvailabilityMapper _personAvailabilityMapper;
        private DataRow oldRow;
        private IPerson person;
        private IAvailabilityRotation _availability;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }

        private void SetupObjectPairs()
        {
            _mappedObjectPair = new MappedObjectPair();
            _personAvailabilities = new ObjectPairCollection<DataRow, IPersonAvailability>();
            ObjectPairCollection<global::Domain.Agent, IPerson> _emps = new ObjectPairCollection<Agent, IPerson>();

            Agent agent = new Agent(1, "Tina", "Vampys", "sdfdf", "dlf", null, null, null, "");
            person = new Domain.Common.Person();
            _emps.Add(agent, person);

            ObjectPairCollection<DataRow, IAvailabilityRotation> availability = new ObjectPairCollection<DataRow, IAvailabilityRotation>();

            DataTable t = new DataTable("table1");
            t.Locale = CultureInfo.CurrentCulture;
            t.Columns.Add("core_id", typeof(int));
            //t.Columns.Add("rotation_name", typeof(string));
            t.Columns.Add("periods", typeof(int));
            DataRow availabilityRow = t.NewRow();
            availabilityRow["core_id"] = 10;
            //rotRow["rotation_name"] = "Ola";
            availabilityRow["periods"] = 2;

            _availability = new AvailabilityRotation("Availability", 27);
            availability.Add(availabilityRow, _availability);

            _mappedObjectPair.PersonAvailability = _personAvailabilities;
            _mappedObjectPair.Agent = _emps;
            _mappedObjectPair.Availability = availability;

        }

        [SetUp]
        public void Setup()
        {
            SetupObjectPairs();

            DataTable t3 = new DataTable("table2");
            t3.Locale = CultureInfo.CurrentCulture;
            t3.Columns.Add("core_id", typeof(int));
            t3.Columns.Add("emp_id", typeof(int));
            t3.Columns.Add("date_from", typeof(DateTime));

            oldRow = t3.NewRow();

            oldRow["core_id"] = 10;
            oldRow["emp_id"] = 1;
            oldRow["date_from"] = new DateTime(2008, 1, 1);

            _personAvailabilityMapper = new PersonAvailabilityMapper(_mappedObjectPair);
        }

	    [Test]
	    public void ShouldNotMapIfPersonNotPresent()
	    {
		    const int notExistingEmpId = 123;
				var t3 = new DataTable("table2") {Locale = CultureInfo.CurrentCulture};
		    t3.Columns.Add("core_id", typeof(int));
				t3.Columns.Add("emp_id", typeof(int));
				t3.Columns.Add("date_from", typeof(DateTime));

				oldRow = t3.NewRow();

				oldRow["core_id"] = 10;
				oldRow["emp_id"] = notExistingEmpId;
				oldRow["date_from"] = new DateTime(2008, 1, 1);

		    _personAvailabilityMapper.Map(oldRow)
			    .Should().Be.Null();
	    }

        [Test]
        public void VerifyGetObjectPairCollection()
        {
            Assert.AreSame(_personAvailabilities, _mappedObjectPair.PersonAvailability);
        }

        [Test]
        public void VerifyConversion()
        {
            IPersonAvailability newPerssonAvailability = _personAvailabilityMapper.Map(oldRow);
            //Assert.AreEqual(7, newPerssonAvailability.StartDay);
            Assert.AreEqual(new DateOnly(2008, 1, 1), newPerssonAvailability.StartDate);
            Assert.AreSame(person, newPerssonAvailability.Person);
            Assert.AreSame(_availability, newPerssonAvailability.Availability);
        }
    }
}
