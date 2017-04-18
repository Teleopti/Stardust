using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	/// <summary>
	/// Tests for PersonPeriodModel
	/// </summary>
	[TestFixture]
	public class PersonPeriodModelTest : PeopleAdminTestBase
	{
		private PersonPeriodModel _target;
		private IPersonPeriod _personPeriod1, _personPeriod2, _personPeriod3;
		private CommonNameDescriptionSetting commonNameDecroption1;

		private PersonPeriodModel _target2;
		private PersonPeriodModel _target3;

		[SetUp]
		public void Setup()
		{
			IPerson person = PersonFactory.CreatePerson("Mama", "Hawa");

			CreateSkills();

			CreatePersonSkillCollection();

			CreateExternalLogOnCollection();

			CreateSiteTeamCollection();


			_personPeriod1 = PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly1,
																			  Skill1);
			_personPeriod2 = PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly2, Skill2);
			_personPeriod3 = PersonPeriodFactory.CreatePersonPeriodWithSkills(DateOnly3, Skill3);

			person.AddExternalLogOn(ExternalLogOn1, _personPeriod1);
			person.AddExternalLogOn(ExternalLogOn2, _personPeriod2);
			person.AddExternalLogOn(ExternalLogOn3,_personPeriod3);

			_personPeriod1.RuleSetBag = new RuleSetBag();
			_personPeriod2.RuleSetBag = new RuleSetBag();
			_personPeriod3.RuleSetBag = new RuleSetBag();

			_personPeriod1.BudgetGroup = new BudgetGroup();
			_personPeriod2.BudgetGroup = new BudgetGroup();
			_personPeriod3.BudgetGroup = new BudgetGroup();

			person.AddPersonPeriod(_personPeriod1);
			person.AddPersonPeriod(_personPeriod2);
			person.AddPersonPeriod(_personPeriod3);

			commonNameDecroption1 = new CommonNameDescriptionSetting();


			_target = new PersonPeriodModel(DateOnly2, person, PersonSkillCollection,
																			ExternalLogOnCollection,
																			SiteTeamAdapterCollection, null);

			_target2 = new PersonPeriodModel(DateOnly1, person, PersonSkillCollection,
																		   ExternalLogOnCollection,
																		   SiteTeamAdapterCollection, null);

			_target3 = new PersonPeriodModel(DateOnly1, person, PersonSkillCollection,
																		   ExternalLogOnCollection,
																		   SiteTeamAdapterCollection, commonNameDecroption1);
		}

		[Test]
		public void VerifyGridControlCanSet()
		{
            using (GridControl grid = new GridControl())
            {
                _target.GridControl = grid;
                Assert.IsNotNull(_target.GridControl);
            }
		}

		[Test]
		public void VerifyPropertiesNotNullOrEmpty()
		{
			Assert.IsNotNull(_target.Parent);
			Assert.IsNotEmpty(_target.FullName);
			Assert.IsNotNull(_target.PeriodDate);
			Assert.IsNotNull(_target.Contract);
			Assert.IsNotNull(_target.ContractSchedule);
			Assert.IsNotNull(_target.PersonContract);
			Assert.IsNotNull(_target.PartTimePercentage);
			Assert.IsNotNull(_target.RuleSetBag);
			Assert.IsFalse(_target.CanGray);
			Assert.IsFalse(_target.CanBold);
			Assert.IsNotNull(_target.PeriodCount);
			Assert.IsNotNull(_target.Period);
			Assert.IsNotNull(_target.ExternalLogOnNames);
			Assert.IsNull(_target.Note);


			//If current period is null 
			_target.GetCurrentPersonPeriodByDate(new DateOnly(DateTime.MinValue));
			Assert.IsNull(_target.PeriodDate);
			Assert.IsNull(_target.Contract);
			Assert.IsNull(_target.ContractSchedule);
			Assert.IsNull(_target.PersonContract);
			Assert.IsNull(_target.PartTimePercentage);
			Assert.IsNull(_target.RuleSetBag);
			Assert.IsTrue(_target.CanGray);
			Assert.IsNotNull(_target.PeriodCount);
			Assert.IsNull(_target.Period);
			Assert.IsEmpty(_target.ExternalLogOnNames);
			Assert.IsEmpty(_target.Note);
		}

		[Test]
		public void VerifyPeriodDateCanSet()
		{
			//DateTime startTime = DateTime.MinValue.AddYears(1900);
			//Changed to 2006 from 2001, 2001 is not realistic because
			//it is the personperiod list on the Person so there should be 
			//another adapter mapped to that date

			DateOnly startTime = new DateOnly(2006, 1, 1);
			_target.PeriodDate = startTime;

			Assert.AreEqual(startTime, _target.PeriodDate);

			// Setting current period date is null.
			_target.PeriodDate = null;

			Assert.AreEqual(startTime, _target.PeriodDate);

		}

		[Test]
		public void VerifyPeriodDateCannotBeSetToSameAsOtherPeriod()
		{
			DateOnly dateOnly = DateOnly2.AddDays(10);
			_target.Parent.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(dateOnly));
			_target.PeriodDate = dateOnly;

			Assert.AreEqual(dateOnly.AddDays(1), _target.PeriodDate);
		}

		[Test]
		public void VerifyCurrentRuleSetBagCanSet()
		{
			RuleSetBag ruleSetBag = new RuleSetBag();
			_target.RuleSetBag = ruleSetBag;

			Assert.AreEqual(ruleSetBag, _target.RuleSetBag);

		}

		[Test]
		public void VerifyCurrentBudgetGroupCanSet()
		{
			BudgetGroup budgetGroup = new BudgetGroup();
			_target.BudgetGroup = budgetGroup;

			Assert.AreEqual(budgetGroup, _target.BudgetGroup);

		}

		[Test]
		public void VerifyCurrentContractCanSet()
		{
			IContract contract = ContractFactory.CreateContract("Contract1");
			_target.Contract = contract;

			Assert.AreEqual(contract, _target.Contract);
		}

		[Test]
		public void VerifyCurrentPersonContractCanSet()
		{
			IPersonContract personContract = PersonContractFactory.CreatePersonContract();
			_target.PersonContract = personContract;

			Assert.AreEqual(personContract, _target.PersonContract);
		}

		[Test]
		public void VerifyCurrentPartTimePercentageCanSet()
		{
			IPartTimePercentage partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("PartTimePercentage1");
			_target.PartTimePercentage = partTimePercentage;

			Assert.AreEqual(partTimePercentage, _target.PartTimePercentage);
		}

		[Test]
		public void VerifyCurrentContractScheduleCanSet()
		{
			IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("ContractSchedule1");
			_target.ContractSchedule = contractSchedule;

			Assert.AreEqual(contractSchedule, _target.ContractSchedule);
		}

		[Test]
		public void VerifyExpandStateCanSet()
		{
			Assert.AreEqual(false, _target.ExpandState);
			_target.ExpandState = true;
			Assert.AreEqual(true, _target.ExpandState);
		}

		[Test]
		public void VerifyPersonSkillCanGetAndSet()
		{
			IPersonPeriod currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyInRange);

			Assert.AreEqual(_personPeriod2, currentPeriod);
			Assert.AreEqual(Skill2, currentPeriod.PersonSkillCollection.First().Skill);
			Assert.AreEqual("_skill2", _target.PersonSkills);

			Assert.AreEqual(1, currentPeriod.PersonSkillCollection.Count());
			_target.PersonSkills = "_skill1";
			Assert.AreEqual(1, currentPeriod.PersonSkillCollection.Count());
			Assert.AreEqual("_skill1", _target.PersonSkills);
		}

		[Test]
		public void VerifyGetCurrentPersonPeriodByDate()
		{
			IPersonPeriod currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyNotInRange);
			Assert.IsNull(currentPeriod);

			currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyInRange);
			Assert.IsNotNull(currentPeriod);
		}

		[Test]
		public void VerifyPersonExternalLogOnNamesCanGetAndSet()
		{
			IPersonPeriod currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyInRange);

			Assert.AreEqual(_personPeriod2, currentPeriod);
			Assert.AreEqual(ExternalLogOn2, currentPeriod.ExternalLogOnCollection.First());
			Assert.AreEqual("Login name (DS)", _target.ExternalLogOnNames);

			Assert.AreEqual(1, currentPeriod.ExternalLogOnCollection.Count());
			_target.ExternalLogOnNames = "Login name (DS)";
			Assert.AreEqual(3, currentPeriod.ExternalLogOnCollection.Count());
            Assert.AreEqual("Login name (DS), Login name (DS), Login name (DS)", _target.ExternalLogOnNames);
		}

		[Test]
		public void VerifySiteTeamCanGetAndSet()
		{
			IPersonPeriod currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyInRange);

			Assert.IsNotNull(currentPeriod);
			currentPeriod.Team = TeamBlue;
			Assert.AreEqual(BLUESITE + "/" + BLUETEAM, _target.SiteTeam.Description);

			_target.SiteTeam = SiteTeam2;
			Assert.AreEqual(REDSITE + "/" + REDTEAM, _target.SiteTeam.Description);

			Assert.IsNull(_target2.SiteTeam);
		}

		[Test]
		public void VerifyNoteCanSet()
		{
			IPersonPeriod currentPeriod = _target.GetCurrentPersonPeriodByDate(DateOnlyInRange);
			Assert.IsNotNull(currentPeriod);

			string note = "Mage Note Eka";
			_target.Note = "Mage Note Eka";
			Assert.AreEqual(note, _target.Note);

		}

		[Test]
		public void VerifyCanSetCanBold()
		{
			Assert.IsFalse(_target.CanBold);
			_target.CanBold = true;
			Assert.IsTrue(_target.CanBold);

		}

		[Test]
		public void VerifyCanGetFullName()
		{
			Assert.AreEqual(_target.FullName, _target3.FullName);
			commonNameDecroption1.AliasFormat = "{LastName} {FirstName}";
			Assert.AreNotEqual(_target.FullName, _target3.FullName);

		}

		[Test]
		public void VerifyResetCanBoldPropertyOfChildAdapters()
		{
            using (GridControl grid = new GridControl())
            {
                PersonPeriodChildModel adapter1 = EntityConverter.ConvertToOther<IPersonPeriod,
                    PersonPeriodChildModel>(_personPeriod1);

                PersonPeriodChildModel adapter2 = EntityConverter.ConvertToOther<IPersonPeriod,
                    PersonPeriodChildModel>(_personPeriod2);


                adapter1.CanBold = true;
                adapter2.CanBold = true;

                IList<PersonPeriodChildModel> adapterCollection = new
                    List<PersonPeriodChildModel>();
                adapterCollection.Add(adapter1);
                adapterCollection.Add(adapter2);

                grid.Tag = adapterCollection;

                _target.GridControl = grid;

                _target.ResetCanBoldPropertyOfChildAdapters();

                IList<PersonPeriodChildModel> childAdapters = _target.GridControl.Tag as
                                                              IList<PersonPeriodChildModel>;


                Assert.IsNotNull(childAdapters);
                Assert.AreEqual(2, childAdapters.Count);
                Assert.IsFalse(childAdapters[0].CanBold);
                Assert.IsFalse(childAdapters[1].CanBold);
            }
		}

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenChildCanBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(_personPeriod1);
				adapter1.CanBold = true;
				IList<PersonPeriodChildModel> adapterCollection = new List<PersonPeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;

				_target.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldGetCanBoldOnAdapterAndChildAdaptersWhenParentCanBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(_personPeriod1);
				IList<PersonPeriodChildModel> adapterCollection = new List<PersonPeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;
				_target.CanBold = true;
				_target.AdapterOrChildCanBold().Should().Be.True();
			}
		}

		[Test]
		public void ShouldNotGetCanBoldOnAdapterAndChildAdaptersWhenParentOrChildCantBold()
		{
			using (var grid = new GridControl())
			{
				var adapter1 = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(_personPeriod1);
				IList<PersonPeriodChildModel> adapterCollection = new List<PersonPeriodChildModel>();
				adapterCollection.Add(adapter1);
				grid.Tag = adapterCollection;
				_target.GridControl = grid;
				_target.AdapterOrChildCanBold().Should().Be.False();
			}
		}

		[Test]
		public void VerifyPeriodCount()
		{
			IPerson person = GetPersonWithOnePeriod();

			PersonPeriodModel adapter1 = new PersonPeriodModel(DateOnly2, person,
																				PersonSkillCollection,
                                                                            ExternalLogOnCollection, null, null);
			Assert.AreEqual(0, adapter1.PeriodCount);
		}

		private IPerson GetPersonWithOnePeriod()
		{
			IPerson person = PersonFactory.CreatePerson();
			person.AddPersonPeriod(_personPeriod1);
			return person;
		}
	}
}
