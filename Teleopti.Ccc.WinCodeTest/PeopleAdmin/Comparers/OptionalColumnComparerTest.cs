using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Test class for theOptionalColumnComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 8/13/2008
    /// </remarks>
    [TestFixture]
    public class OptionalColumnComparerTest
    {
		private IPerson person;
		private IPersonPeriod personPeriod;
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private OptionalColumnComparer<PersonGeneralModel> optionalColumnComparer;

        private IList<IOptionalColumn> optionalColumns;

		private int result;
        private PrincipalAuthorization _principalAuthorization;

        /// <summary>
		/// Tests the init.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[SetUp]
		public void TestInit()
		{
			// Instantiates the person and teh team
			person = PersonFactory.CreatePerson();
			ITeam team = TeamFactory.CreateSimpleTeam();
			// Creates the person period
			personPeriod = PersonPeriodFactory.CreatePersonPeriod
				(new DateOnly(DateTime.Now),
				 PersonContractFactory.CreatePersonContract("testContract", "TestSchedule", "TestPartTimePercentage"),
				 team);
			person.AddPersonPeriod(personPeriod);
            _principalAuthorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal());
			// Ses the contained entity
            _target = new PersonGeneralModel(person, new UserDetail(person), _principalAuthorization);

			// Instantiates the person and teh team
			IPerson person1 = PersonFactory.CreatePerson();
			ITeam team1 = TeamFactory.CreateSimpleTeam();
			// Creates the person period
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod
				(new DateOnly(DateTime.Now).AddDays(1),
				 PersonContractFactory.CreatePersonContract("testContract1", "TestSchedule1", "TestPartTimePercentage1"),
				 team1);
			person.AddPersonPeriod(personPeriod1);
			// Ses the contained entity
            _personGeneralModel = new PersonGeneralModel(person1, new UserDetail(person1), _principalAuthorization);
		}

        /// <summary>
        /// Verifies the compare method with null values for all parameters.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithNoOptionalColumn()
        {             
            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with null values for all parameters.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            // Sets the user Id
            _target.ContainedEntity.SetId(Guid.NewGuid());
            _personGeneralModel.ContainedEntity.SetId(Guid.NewGuid());

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            IOptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);
            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the first parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            // Sets the user Id
            _target.ContainedEntity.SetId(id1);
            _personGeneralModel.ContainedEntity.SetId(id2);

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            IOptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");

            IOptionalColumnValue value = new OptionalColumnValue("FirstNull");
            value.ReferenceId = id2;
            optionalCoumn.AddOptionalColumnValue(value);
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);

            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the second parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            // Sets the user Id
            _target.ContainedEntity.SetId(id1);
            _personGeneralModel.ContainedEntity.SetId(id2);

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            OptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");

            OptionalColumnValue value = new OptionalColumnValue("FirstNull");
            value.ReferenceId = id1;
            optionalCoumn.AddOptionalColumnValue(value);
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);
            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with a for the first parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodAscending()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            // Sets the user Id
            _target.ContainedEntity.SetId(id1);
            _personGeneralModel.ContainedEntity.SetId(id2);

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            OptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");

            OptionalColumnValue value = new OptionalColumnValue("FirstNull A");
            value.ReferenceId = id1;
            optionalCoumn.AddOptionalColumnValue(value);
            optionalColumns.Add(optionalCoumn);

            OptionalColumnValue value1 = new OptionalColumnValue("FirstNull B");
            value1.ReferenceId = id2;
            optionalCoumn.AddOptionalColumnValue(value1);
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);
            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with a for teh second parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodDescending()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            // Sets the user Id
            _target.ContainedEntity.SetId(id1);
            _personGeneralModel.ContainedEntity.SetId(id2);

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            OptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");

            OptionalColumnValue value = new OptionalColumnValue("FirstNull B");
            value.ReferenceId = id1;
            optionalCoumn.AddOptionalColumnValue(value);
            optionalColumns.Add(optionalCoumn);

            OptionalColumnValue value1 = new OptionalColumnValue("FirstNull A");
            value1.ReferenceId = id2;
            optionalCoumn.AddOptionalColumnValue(value1);
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);
            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with same role for both parameters.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 29-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            // Sets the user Id
            _target.ContainedEntity.SetId(id1);
            _personGeneralModel.ContainedEntity.SetId(id2);

            // Creates the optional column data
            optionalColumns = new List<IOptionalColumn>();
            OptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");

            OptionalColumnValue value = new OptionalColumnValue("FirstNull A");
            value.ReferenceId = id1;
            optionalCoumn.AddOptionalColumnValue(value);
            optionalColumns.Add(optionalCoumn);

            OptionalColumnValue value1 = new OptionalColumnValue("FirstNull A");
            value1.ReferenceId = id2;
            optionalCoumn.AddOptionalColumnValue(value1);
            optionalColumns.Add(optionalCoumn);

            _target.SetOptionalColumns(optionalColumns);
            _personGeneralModel.SetOptionalColumns(optionalColumns);

            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
            result = optionalColumnComparer.Compare(_target, _personGeneralModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifySetBindingProperty()
        {
            const string bindingProperty = "FirstColumn";
            // Calls the compares method
            optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>(bindingProperty);

            // Checks whether the roles are equal
            Assert.AreEqual(optionalColumnComparer.BindingProperty, bindingProperty);
        }
    }
}
