using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	[TestFixture]
	public class OptionalColumnComparerTest
	{
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private OptionalColumnComparer<PersonGeneralModel> _optionalColumnComparer;
		private IList<IOptionalColumn> _optionalColumns;
		private int _result;
		private PrincipalAuthorization _authorize;
		private IPerson _person1;


		[SetUp]
		public void TestInit()
		{
			// Instantiates the person and teh team
			_person = PersonFactory.CreatePerson();
			ITeam team = TeamFactory.CreateSimpleTeam();
			// Creates the person period
			_personPeriod = PersonPeriodFactory.CreatePersonPeriod
				(DateOnly.Today,
				 PersonContractFactory.CreatePersonContract("testContract", "TestSchedule", "TestPartTimePercentage"),
				 team);
			_person.AddPersonPeriod(_personPeriod);
			_authorize = new PrincipalAuthorization(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
			// Ses the contained entity
			_target = new PersonGeneralModel(_person, _authorize,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());

			// Instantiates the person and teh team
			_person1 = PersonFactory.CreatePerson();
			ITeam team1 = TeamFactory.CreateSimpleTeam();
			// Creates the person period
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod
				(DateOnly.Today.AddDays(1),
				 PersonContractFactory.CreatePersonContract("testContract1", "TestSchedule1", "TestPartTimePercentage1"),
				 team1);
			_person.AddPersonPeriod(personPeriod1);
			// Ses the contained entity
			_personGeneralModel = new PersonGeneralModel(_person1, _authorize,
				new PersonAccountUpdaterDummy(), new LogonInfoModel(), new PasswordPolicyFake());
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
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, _result);
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
			_optionalColumns = new List<IOptionalColumn>();
			IOptionalColumn optionalCoumn = new OptionalColumn("FirstColumn");
			_optionalColumns.Add(optionalCoumn);

			_target.SetOptionalColumns(_optionalColumns);
			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, _result);
		}

		[Test]
		public void VerifyCompareMethodWithFirstNull()
		{
			Guid id1 = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();

			// Sets the user Id
			_target.ContainedEntity.SetId(id1);
			_personGeneralModel.ContainedEntity.SetId(id2);

			// Creates the optional column data
			_optionalColumns = new List<IOptionalColumn>();
			IOptionalColumn optionalColumn = new OptionalColumn("FirstColumn");

			IOptionalColumnValue value = new OptionalColumnValue("FirstNull");
			_person.SetOptionalColumnValue(value, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			_target.SetOptionalColumns(_optionalColumns);

			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			Assert.AreEqual(1, _result);
		}

		[Test]
		public void VerifyCompareMethodWithSecondNull()
		{
			Guid id1 = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();

			// Sets the user Id
			_target.ContainedEntity.SetId(id1);
			_personGeneralModel.ContainedEntity.SetId(id2);

			// Creates the optional column data
			_optionalColumns = new List<IOptionalColumn>();
			var optionalColumn = new OptionalColumn("FirstColumn");

			var value = new OptionalColumnValue("FirstNull");

			_person.SetOptionalColumnValue(value, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			_target.SetOptionalColumns(_optionalColumns);
			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, _result);
		}

		[Test]
		public void VerifyCompareMethodAscending()
		{
			Guid id1 = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();
			var person = new Person();

			// Sets the user Id
			_target.ContainedEntity.SetId(id1);
			_personGeneralModel.ContainedEntity.SetId(id2);

			// Creates the optional column data
			_optionalColumns = new List<IOptionalColumn>();
			var optionalColumn = new OptionalColumn("FirstColumn");

			var value = new OptionalColumnValue("FirstNull A");
			person.SetOptionalColumnValue(value, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			var value1 = new OptionalColumnValue("FirstNull B");

			_person1.SetOptionalColumnValue(value1, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			_target.SetOptionalColumns(_optionalColumns);
			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, _result);
		}


		[Test]
		public void VerifyCompareMethodDescending()
		{
			Guid id1 = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();

			// Sets the user Id
			_target.ContainedEntity.SetId(id1);
			_personGeneralModel.ContainedEntity.SetId(id2);

			// Creates the optional column data
			_optionalColumns = new List<IOptionalColumn>();
			var optionalColumn = new OptionalColumn("FirstColumn");

			var value = new OptionalColumnValue("FirstNull B");
			_person.SetOptionalColumnValue(value, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			var value1 = new OptionalColumnValue("FirstNull A");
			_person1.SetOptionalColumnValue(value1, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			_target.SetOptionalColumns(_optionalColumns);
			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, _result);
		}

		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
			Guid id1 = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();

			// Sets the user Id
			_target.ContainedEntity.SetId(id1);
			_personGeneralModel.ContainedEntity.SetId(id2);

			// Creates the optional column data
			_optionalColumns = new List<IOptionalColumn>();
			var optionalColumn = new OptionalColumn("FirstColumn");

			var value = new OptionalColumnValue("FirstNull A");
			_person.SetOptionalColumnValue(value, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			var value1 = new OptionalColumnValue("FirstNull A");
			_person1.SetOptionalColumnValue(value1, optionalColumn);
			_optionalColumns.Add(optionalColumn);

			_target.SetOptionalColumns(_optionalColumns);
			_personGeneralModel.SetOptionalColumns(_optionalColumns);

			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>("FirstColumn");
			_result = _optionalColumnComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, _result);
		}

		[Test]
		public void VerifySetBindingProperty()
		{
			const string bindingProperty = "FirstColumn";
			// Calls the compares method
			_optionalColumnComparer = new OptionalColumnComparer<PersonGeneralModel>(bindingProperty);

			// Checks whether the roles are equal
			Assert.AreEqual(_optionalColumnComparer.BindingProperty, bindingProperty);
		}
	}
}
