using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.PeopleAdmin.Views;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class FilteredPeopleHolder : IDisposable
	{
		private ITraceableRefreshService _refreshService;
		private readonly IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
		private readonly ITenantDataManager _tenantDataManager;
		private readonly List<IPerson> _personCollection = new List<IPerson>();
		private readonly List<IPerson> _filteredPersonCollection = new List<IPerson>();
		private List<PersonGeneralModel> _filteredPeopleGridData = new List<PersonGeneralModel>();
		private IList<PersonPeriodModel> _personPeriodGridViewCollection = new List<PersonPeriodModel>();
		private readonly List<IPersonSkill> _personSkillCollection = new List<IPersonSkill>();
		private CommonNameDescriptionSetting _commonNameDescription;
		private readonly List<IExternalLogOn> _externalLogOnCollection = new List<IExternalLogOn>();
		private IList<SchedulePeriodModel> _schedulePeriodGridViewCollection = new List<SchedulePeriodModel>();
		private readonly IList<IPersonRotation> _allPersonRotationCollection = new List<IPersonRotation>();
		private readonly IList<IPersonAvailability> _allPersonAvailabilityCollection = new List<IPersonAvailability>();
		private IList<PersonRotationModelParent> _personRotationParentAdapterCollection = new List<PersonRotationModelParent>();
		private IList<PersonAvailabilityModelParent> _personAvailabilityParentAdapterCollection = new List<PersonAvailabilityModelParent>();
		private readonly IList<IPersonRotation> _parentPersonRotationCollection = new List<IPersonRotation>();
		private readonly IList<IPersonAvailability> _parentPersonAvailabilityCollection = new List<IPersonAvailability>();
		private TypedBindingCollection<IRotation> _rotationCollection = new TypedBindingCollection<IRotation>();
		private TypedBindingCollection<IAvailabilityRotation> _availabilityCollection = new TypedBindingCollection<IAvailabilityRotation>();
		private IList<IPersonAccountModel> _personAccountGridViewAdaptorCollection = new List<IPersonAccountModel>();
		private ReadOnlyCollection<IPersonPeriodModel> _selectedPeoplePeriodGridData;
		private IList<IOptionalColumn> _optionalColumnCollection = new List<IOptionalColumn>();
		private readonly TypedBindingCollection<IRuleSetBag> _ruleSetBagBindingCollection = new TypedBindingCollection<IRuleSetBag>();
		private readonly TypedBindingCollection<IBudgetGroup> _budgetGroupBindingCollection = new TypedBindingCollection<IBudgetGroup>();
		private readonly List<PersonSkillModel> _personSkillAdapterCollection = new List<PersonSkillModel>();
		private readonly List<ExternalLogOnModel> _externalLogOnAdapterCollection = new List<ExternalLogOnModel>();
		private readonly TypedBindingCollection<SiteTeamModel> _siteTeamBindingCollection = new TypedBindingCollection<SiteTeamModel>();
		private readonly Collection<IPerson> _validateUserCredentialsCollection = new Collection<IPerson>();
		private readonly IList<IPersonRotation> _newPersonRotationCollection = new List<IPersonRotation>();
		private readonly IList<IPersonAvailability> _newPersonAvailabilityCollection = new List<IPersonAvailability>();
		private readonly List<IAbsence> _absenceCollection = new List<IAbsence>();
		private List<IAbsence> _filteredAbsenceCollection = new List<IAbsence>();
		private readonly IList<PersonGeneralModel> _validatePasswordPolicy = new List<PersonGeneralModel>();
		private readonly List<IApplicationRole> _applicationRoleCollection = new List<IApplicationRole>();
		private readonly List<RolesModel> _rolesViewAdapterCollection = new List<RolesModel>();
		private ReadOnlyCollection<PersonGeneralModel> _selectedPeopleGeneralGridData;
		private IList<ExternalLogOnModel> _filteredExternalLogOnCollection;

		private IList<Guid> toBeRemovedList = new List<Guid>();
		private IEnumerable<LogonInfoModel> _logonData;

		public FilteredPeopleHolder(ITraceableRefreshService refreshService,
				IDictionary<IPerson, IPersonAccountCollection> allAccounts,
				ITenantDataManager tenantDataManager)
		{
			_refreshService = refreshService;
			_allAccounts = allAccounts;
			_tenantDataManager = tenantDataManager;
		}

		//public void SetState(ITraceableRefreshService refreshService,
		//	  IUnitOfWork unitOfWork,
		//							 IDictionary<IPerson, IPersonAccountCollection> allAccounts)
		// {
		//	  clearCollections();

		//	 _refreshService = refreshService;
		//	  GetUnitOfWork = unitOfWork;
		//	  _allAccounts.Clear();
		//	  foreach (var allAccount in allAccounts)
		//	  {
		//			_allAccounts.Add(allAccount.Key,allAccount.Value);
		//	  }
		// }

		public ReadOnlyCollection<PersonGeneralModel> SelectedPeopleGeneralGridData
		{
			get { return _selectedPeopleGeneralGridData; }
		}

		public IUnitOfWork GetUnitOfWork { get; set; }

		public IPersonAccountCollection GetPersonAccounts(IPerson person)
		{
			IPersonAccountCollection personAccountCollection = new PersonAccountCollection(person);
			var accounts = AllAccounts.Where(p => p.Key == person).ToArray();
			foreach (var account in accounts.SelectMany(keyValuePair => keyValuePair.Value))
			{
				personAccountCollection.Add(account);
			}
			return personAccountCollection;
		}

		public Collection<IPerson> PersonCollection
		{
			get { return new Collection<IPerson>(_personCollection); }
		}

		public TypedBindingCollection<SiteTeamModel> SiteTeamAdapterCollection
		{
			get { return _siteTeamBindingCollection; }
		}

		public Collection<PersonGeneralModel> PeopleGridData
		{
			get
			{
				var peopleGridData = new List<PersonGeneralModel>();
				peopleGridData.AddRange(_personCollection.ConvertAll((EntityConverter.ConvertToOther<IPerson, PersonGeneralModel>)));
				return new Collection<PersonGeneralModel>(peopleGridData);
			}
		}

		public Collection<IPerson> FilteredPersonCollection
		{
			get { return new Collection<IPerson>(_filteredPersonCollection); }
		}

		public Collection<PersonGeneralModel> FilteredPeopleGridData
		{
			get { return new Collection<PersonGeneralModel>(_filteredPeopleGridData); }
		}

		public Collection<PersonPeriodModel> PersonPeriodGridViewCollection
		{
			get { return new Collection<PersonPeriodModel>(_personPeriodGridViewCollection); }
		}

		public Collection<SchedulePeriodModel> SchedulePeriodGridViewCollection
		{
			get { return new Collection<SchedulePeriodModel>(_schedulePeriodGridViewCollection); }
		}

		public Collection<IPersonRotation> ParentPersonRotationCollection
		{
			get { return new Collection<IPersonRotation>(_parentPersonRotationCollection); }
		}

		public Collection<IPersonAvailability> ParentPersonAvailabilityCollection
		{
			get { return new Collection<IPersonAvailability>(_parentPersonAvailabilityCollection); }
		}

		public Collection<PersonRotationModelParent> PersonRotationParentAdapterCollection
		{
			get { return new Collection<PersonRotationModelParent>(_personRotationParentAdapterCollection); }
		}

		public Collection<PersonAvailabilityModelParent> PersonAvailabilityParentAdapterCollection
		{
			get { return new Collection<PersonAvailabilityModelParent>(_personAvailabilityParentAdapterCollection); }
		}

		public Collection<IPersonRotation> AllPersonRotationCollection
		{
			get { return new Collection<IPersonRotation>(_allPersonRotationCollection); }
		}

		public Collection<IPersonAvailability> AllPersonAvailabilityCollection
		{
			get { return new Collection<IPersonAvailability>(_allPersonAvailabilityCollection); }
		}

		public Collection<IPersonAccountModel> PersonAccountModelCollection
		{
			get { return new Collection<IPersonAccountModel>(_personAccountGridViewAdaptorCollection); }
		}

		public DateOnly SelectedDate { get; set; }

		public CommonNameDescriptionSetting CommonNameDescription
		{
			get { return _commonNameDescription; }
		}

		public IAbsence SelectedPersonAccountAbsenceType { get; set; }

		public ReadOnlyCollection<IPersonPeriodModel> SelectedPeoplePeriodGridCollection
		{
			get { return _selectedPeoplePeriodGridData; }
		}

		public void AddNewPersonRotation(IPersonRotation personRotation)
		{
			InParameter.NotNull("personRotation", personRotation);

			_newPersonRotationCollection.Add(personRotation);

		}

		public bool DeleteNewPersonRotation(IPersonRotation personRotation)
		{
			bool isDeleted = false;

			if (_newPersonRotationCollection.Contains(personRotation))
			{
				_newPersonRotationCollection.Remove(personRotation);
				isDeleted = true;
			}

			return isDeleted;
		}

		public void AddNewPersonAvailability(IPersonAvailability personAvailability)
		{
			InParameter.NotNull("personAvailability", personAvailability);

			_newPersonAvailabilityCollection.Add(personAvailability);

		}

		public bool DeleteNewPersonAvailability(IPersonAvailability personAvailability)
		{
			bool isDeleted = false;

			if (_newPersonAvailabilityCollection.Contains(personAvailability))
			{
				_newPersonAvailabilityCollection.Remove(personAvailability);
				isDeleted = true;
			}

			return isDeleted;
		}

		public Collection<IOptionalColumn> OptionalColumnCollection
		{
			get { return new Collection<IOptionalColumn>(_optionalColumnCollection); }
		}

		public TypedBindingCollection<IRuleSetBag> RuleSetBagCollection
		{
			get { return _ruleSetBagBindingCollection; }
		}

		public TypedBindingCollection<IBudgetGroup> BudgetGroupCollection
		{
			get { return _budgetGroupBindingCollection; }
		}

		public ReadOnlyCollection<IExternalLogOn> ExternalLogOnCollection
		{
			get { return new ReadOnlyCollection<IExternalLogOn>(_externalLogOnCollection); }
		}

		public IList<ExternalLogOnModel> FilteredExternalLogOnCollection
		{
			get { return _filteredExternalLogOnCollection; }
		}

		public void SetFilteredExternalLogOnCollection(IList<ExternalLogOnModel> externalLogOnViewAdapters)
		{
			_filteredExternalLogOnCollection = externalLogOnViewAdapters;
		}

		public ReadOnlyCollection<ExternalLogOnModel> FilteredExternalLogOnCollectionCellStages
		{
			get { return new ReadOnlyCollection<ExternalLogOnModel>(_filteredExternalLogOnCollection); }
		}

		public Collection<IPersonSkill> PersonSkillCollection
		{
			get { return new Collection<IPersonSkill>(_personSkillCollection); }
		}

		public ReadOnlyCollection<ExternalLogOnModel> ExternalLogOnViewAdapterCollection
		{
			get { return new ReadOnlyCollection<ExternalLogOnModel>(_externalLogOnAdapterCollection); }
		}

		public ReadOnlyCollection<PersonSkillModel> PersonSkillViewAdapterCollection
		{
			get { return new ReadOnlyCollection<PersonSkillModel>(_personSkillAdapterCollection); }
		}


		public TabControlAdv TabControlPeopleAdmin { get; set; }

		public Collection<IPerson> ValidateUserCredentialsCollection { get { return _validateUserCredentialsCollection; } }

		public ReadOnlyCollection<IAbsence> AbsenceCollection
		{
			get
			{
				return new ReadOnlyCollection<IAbsence>(_absenceCollection);
			}
		}

		public TypedBindingCollection<IAbsence> FilteredAbsenceCollection
		{
			get
			{
				var trackerCollection = new TypedBindingCollection<IAbsence>();

				foreach (IAbsence absence in _filteredAbsenceCollection)
				{
					trackerCollection.Add(absence);
				}

				return trackerCollection;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Uow")]
		public void ReassociateSelectedPeopleWithNewUow(IEnumerable<Guid> peopleId)
		{
			InParameter.NotNull("peopleId", peopleId);

			var rep = new PersonRepository(GetUnitOfWork);
			var personRotationRep = new PersonRotationRepository(GetUnitOfWork);
			var personAvailRep = new PersonAvailabilityRepository(GetUnitOfWork);

			clearCollections();

			var foundPeople = rep.FindPeople(peopleId).ToList();

			_filteredPersonCollection.AddRange(foundPeople);
			var today = DateOnly.Today;

			LoadPersonRotations(foundPeople, today, personRotationRep);
			LoadPersonAvailabilities(foundPeople, personAvailRep);

			var repositoryFactory = new RepositoryFactory();
			var repository = repositoryFactory.CreateUserDetailRepository(GetUnitOfWork);
			var userDetails = repository.FindByUsers(foundPeople);

			foreach (var person in _filteredPersonCollection)
			{
				IUserDetail ud;
				if (userDetails.ContainsKey(person))
				{
					ud = userDetails[person];
				}
				else
				{
					ud = new UserDetail(person);
					repository.Add(ud);
				}

				loadFilteredPeopleGridData(person, ud, GetLogonInfoModelFromPersonId(person.Id.GetValueOrDefault()));
				getParentPersonPeriods(person, today);
				GetParentSchedulePeriods(person, today);
				GetParentPersonAccounts(person, today);
			}
		}

		public void ReassociateSelectedPeopleWithNewUowOpenPeople(IList<IPerson> people, IEnumerable<LogonInfoModel> logonData)
		{
			_logonData = logonData;
			int length = people.Count();

			if (length > 0)
			{
				var personRotationRep = new PersonRotationRepository(GetUnitOfWork);
				var personAvailRep = new PersonAvailabilityRepository(GetUnitOfWork);

				clearCollections();

				_filteredPersonCollection.AddRange(people);
				_personCollection.AddRange(people);
				var today = DateOnly.Today;

				LoadPersonRotations(people, today, personRotationRep);
				LoadPersonAvailabilities(people, personAvailRep);

				var repositoryFactory = new RepositoryFactory();
				var repository = repositoryFactory.CreateUserDetailRepository(GetUnitOfWork);
				var userDetails = repository.FindByUsers(people);

				foreach (var person in _filteredPersonCollection)
				{
					IUserDetail ud;
					if (userDetails.ContainsKey(person))
					{
						ud = userDetails[person];
					}
					else
					{
						ud = new UserDetail(person);
						repository.Add(ud);
					}

					loadFilteredPeopleGridData(person, ud, GetLogonInfoModelFromPersonId(person.Id.GetValueOrDefault()));
					getParentPersonPeriods(person, today);
					GetParentSchedulePeriods(person, today);
					GetParentPersonAccounts(person, today);
				}
			}
		}

		public LogonInfoModel GetLogonInfoModelFromPersonId(Guid personId)
		{
			return _logonData.FirstOrDefault(logonInfoModel => logonInfoModel.PersonId.Equals(personId));
		}

		private void clearCollections()
		{
			_filteredPersonCollection.Clear();
			_filteredPeopleGridData.Clear();
			_personPeriodGridViewCollection.Clear();
			_schedulePeriodGridViewCollection.Clear();

			_allPersonAvailabilityCollection.Clear();
			_allPersonRotationCollection.Clear();

			_parentPersonRotationCollection.Clear();
			_parentPersonAvailabilityCollection.Clear();

			_personRotationParentAdapterCollection.Clear();
			_personAvailabilityParentAdapterCollection.Clear();

			_personAccountGridViewAdaptorCollection.Clear();
		}

		private void loadFilteredPeopleGridData(IPerson person, IUserDetail userDetail, LogonInfoModel logonInfoModel)
		{
			//create new person grid data.
			var personGridData = new PersonGeneralModel(person, userDetail,
				new PrincipalAuthorization(new CurrentTeleoptiPrincipal()),
				new FilteredPeopleAccountUpdater(this, UnitOfWorkFactory.Current), UnitOfWorkFactory.Current.Name, logonInfoModel);

			//set optional columns if any.
			if (_optionalColumnCollection.Count > 0)
				personGridData.SetOptionalColumns(_optionalColumnCollection);

			personGridData.SetAvailableRoles(_applicationRoleCollection);

			_filteredPeopleGridData.Add(personGridData);
		}

		private void LoadTeams()
		{
			_siteTeamBindingCollection.Clear();
			var repository = new TeamRepository(GetUnitOfWork);
			var list = repository.FindAllTeamByDescription().ToList();

			foreach (ITeam item in list)
			{
				_siteTeamBindingCollection.Add(EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(item));
			}
		}

		public void SetRotationCollection(TypedBindingCollection<IRotation> rotationCollection)
		{
			_rotationCollection = rotationCollection;
		}

		public void SetAvailabilityCollection(TypedBindingCollection<IAvailabilityRotation> availabilityCollection)
		{
			_availabilityCollection = availabilityCollection;
		}

		public void LoadPersonRotations(IList<IPerson> persons, DateOnly selectedDateTime, PersonRotationRepository personRotationRep)
		{
			if (_filteredPersonCollection != null && _rotationCollection.Count > 0)
			{
				//todo Argh... first things first, but this is realy timeconsuming stuff....lankeeze

				var rotationCollection = personRotationRep.Find(persons);
				var sorted = rotationCollection.OrderByDescending(n2 => n2.StartDate);
				rotationCollection = sorted.ToList();

				foreach (IPersonRotation rotation in rotationCollection)
				{
					_allPersonRotationCollection.Add(rotation);
				}

				//one more loop
				foreach (var person in persons)
				{
					IPerson person1 = person;
					var g = rotationCollection.Where(p => p.Person.Equals(person1)).ToList();
					fillRotationAdapterCollection(person, g, selectedDateTime);
				}
			}
		}

		private void fillRotationAdapterCollection(IPerson person, IList<IPersonRotation> rotationCollection, DateOnly selectedDateTime)
		{
			IPersonRotation currentPersonRotation = rotationCollection.Count > 0 ? GetCurrentPersonRotation(rotationCollection) : null;

			if (currentPersonRotation != null)
				_parentPersonRotationCollection.Add(currentPersonRotation);
			else
			{
				IPersonRotation dummyPersonRotation = GetSamplePersonRotation(person, selectedDateTime);
				_parentPersonRotationCollection.Add(dummyPersonRotation);
			}

			//Setup the Person Rotation grid view adapter

			//set the adapter
			var personRotationModel = new PersonRotationModelParent(person, _commonNameDescription);

			if (currentPersonRotation == null)
			{
				personRotationModel.PersonRotation = null;
				personRotationModel.CurrentRotation = null;
			}
			else
			{
				personRotationModel.PersonRotation = currentPersonRotation;
				personRotationModel.CurrentRotation = currentPersonRotation.Rotation;
			}


			//set the Rotation Count. It is necessary to make it 0 when the count it 1 to make sure the + sign doesnt appear
			personRotationModel.RotationCount = rotationCollection.Count == 1 && currentPersonRotation != null ? 0 : rotationCollection.Count;

			//add the adapter to the collection
			_personRotationParentAdapterCollection.Add(personRotationModel);
		}

		public void LoadPersonAvailabilities(IList<IPerson> persons, PersonAvailabilityRepository personAvailabilityRep)
		{
			// Gets the data list
			var period = new DateOnlyPeriod(DateOnly.MinValue,DateOnly.MaxValue);

			if (_filteredPersonCollection != null && _availabilityCollection.Count > 0)
			{

				//todo: fix performance in here!!!!
				var availabilityCollection = personAvailabilityRep.Find(persons, period);

				var sorted = availabilityCollection.OrderByDescending(n2 => n2.StartDate);
				availabilityCollection = sorted.ToList();

				foreach (PersonAvailability avail in availabilityCollection)
				{
					_allPersonAvailabilityCollection.Add(avail);
				}

				IList<IPersonAvailability> availList = new List<IPersonAvailability>(availabilityCollection);

				foreach (var person in persons)
				{
					IPerson person1 = person;
					IList<IPersonAvailability> ava = availList.Where(p => p.Person.Equals(person1)).ToList();
					fillAvailabilityAdapterCollection(person, ava);
				}
			}
		}

		private void fillAvailabilityAdapterCollection(IPerson person, IList<IPersonAvailability> availabilityCollection)
		{
			IPersonAvailability currentPersonAvailability = availabilityCollection.Count > 0 ? GetCurrentPersonAvailability(availabilityCollection) : null;

			if (currentPersonAvailability != null)
				_parentPersonAvailabilityCollection.Add(currentPersonAvailability);
			else
			{
				IPersonAvailability dummyPersonAvailability = GetSamplePersonAvailability(person);
				_parentPersonAvailabilityCollection.Add(dummyPersonAvailability);
			}

			//Setup the Person Rotation grid view adapter

			//set the adapter
			var personAvailabilityAdapter = new PersonAvailabilityModelParent(person, currentPersonAvailability, _commonNameDescription);

			if (currentPersonAvailability == null)
			{
				personAvailabilityAdapter.PersonRotation = null;
				personAvailabilityAdapter.CurrentRotation = null;
			}
			else
			{
				personAvailabilityAdapter.PersonRotation = currentPersonAvailability;
				personAvailabilityAdapter.CurrentRotation = currentPersonAvailability.Availability as AvailabilityRotation;
			}

			//set the Rotation Count. It is necessary to make it 0 when the count it 1 to make sure the + sign doesnt appear
			personAvailabilityAdapter.RotationCount = availabilityCollection.Count == 1 && currentPersonAvailability != null ? 0 : availabilityCollection.Count;

			//add the adapter to the collection
			_personAvailabilityParentAdapterCollection.Add(personAvailabilityAdapter);
		}

		public IPersonAvailability GetSamplePersonAvailability(IPerson person)
		{
			IAvailabilityRotation avail = GetDefaultAvailability;

			var date = new DateOnly(PeopleAdminHelper.GetFirstDayOfWeek(DateTime.Today));

			if (avail != null)
				return new PersonAvailability(person, avail, date);

			return null;
		}

		public void RefreshPersonRotations()
		{
			if (_filteredPersonCollection != null && _rotationCollection.Count > 0)
			{
				//Rotation related collections. //clear the collection first
				_personRotationParentAdapterCollection.Clear();

				foreach (IPerson person in _filteredPersonCollection)
				{
					IPerson person1 = person;
					var rotationCollection = _allPersonRotationCollection.Where(p => p.Person.Equals(person1)).OrderByDescending(n2 => n2.StartDate).ToList();
					fillRotationAdapterCollection(person, rotationCollection, SelectedDate);
				}
			}
		}

		public void RefreshPersonAvailabilities()
		{
			if (_filteredPersonCollection != null && _availabilityCollection.Count > 0)
			{
				//Rotation related collections. //clear the collection first
				_personAvailabilityParentAdapterCollection.Clear();

				foreach (IPerson person in _filteredPersonCollection)
				{
					IPerson person1 = person;
					IList<IPersonAvailability> availCollection = _allPersonAvailabilityCollection.Where(p => p.Person.Equals(person1)).OrderByDescending(n2 => n2.StartDate).ToList();
					fillAvailabilityAdapterCollection(person, availCollection);
				}
			}
		}

		public void AddPersonAccount(int rowIndex)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				if ((AbsenceCollection != null) && (AbsenceCollection.Count > 0))
				{
					var accounts = _personAccountGridViewAdaptorCollection[rowIndex].Parent;
					IAbsence absence = SelectedPersonAccountAbsenceType ?? _filteredAbsenceCollection[0];
					IAccount account = absence.Tracker.CreatePersonAccount(SelectedDate);
					accounts.Add(absence, account);
					UpdatePersonAccounts(account);
				}
			}
		}

		public void AddSchedulePeriod(int rowIndex)
		{
			IPerson personFiltered = _schedulePeriodGridViewCollection[rowIndex].Parent;

			ISchedulePeriod sampleSchedulePeriod = getSampleSchedulePeriod(_schedulePeriodGridViewCollection[rowIndex].SchedulePeriod, personFiltered);

			personFiltered.AddSchedulePeriod(sampleSchedulePeriod);

		}

		private static ISchedulePeriod getSampleSchedulePeriod(ISchedulePeriod currentSchedulePeriod, IPerson person)
		{
			ISchedulePeriod period;
			var date = DateOnly.Today;

			if (currentSchedulePeriod != null)
			{
				period = currentSchedulePeriod.Clone() as ISchedulePeriod;
				if (period != null)
				{
					period.DateFrom = PeriodDateService.GetValidPeriodDate
						 (PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.SchedulePeriodView, person), date);
				}
			}
			else
			{
				period =
					 new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			}

			return period;
		}

		public void DeletePersonAccount(int rowIndex, int personPeriodIndex)
		{

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var accounts = _personAccountGridViewAdaptorCollection[rowIndex].Parent;

				IOrderedEnumerable<IAccount> sorted = accounts.AllPersonAccounts().OrderByDescending(n2 => n2.StartDate.Date);
				IList<IAccount> personAccountSortedCollection = sorted.ToList();

				IAccount account = personAccountSortedCollection[personPeriodIndex];

				if (account != null)
				{
					accounts.Find(account.Owner.Absence).Remove(account);
					UpdatePersonAccounts(account);
				}
			}
		}

		public void SetSortedPersonAccountFilteredList(IList<IPersonAccountModel> result)
		{
			_personAccountGridViewAdaptorCollection = result.ToList();
		}

		public void SetSortedPersonRotationFilteredList(IList<PersonRotationModelParent> result)
		{
			_personRotationParentAdapterCollection = result.ToList();
		}

		public void SetSortedPersonAvailabilityFilteredList(IList<PersonAvailabilityModelParent> result)
		{
			_personAvailabilityParentAdapterCollection = result.ToList();
		}

		public void DeletePersonPeriod(int rowIndex)
		{
			IPerson personFiltered = _filteredPersonCollection[rowIndex];

			personFiltered.RemoveAllPersonPeriods();
		}

		public void DeletePersonPeriod(int rowIndex, DateOnly selectedDateTime)
		{
			IPerson personFiltered = _personPeriodGridViewCollection[rowIndex].Parent;
			IPersonPeriod personPeriod = personFiltered.Period(selectedDateTime);

			if (personPeriod != null)
			{
				personFiltered.DeletePersonPeriod(personPeriod);
				_personPeriodGridViewCollection.RemoveAt(rowIndex);
				_personPeriodGridViewCollection.Insert(rowIndex,
												new PersonPeriodModel(selectedDateTime, personFiltered,
												_personSkillCollection, _externalLogOnCollection,
												_siteTeamBindingCollection, _commonNameDescription));
			}
		}

		public void DeletePersonPeriod(int rowIndex, IPersonPeriod personPeriod)
		{
			if (personPeriod == null) return;

			IPerson personFiltered = _personPeriodGridViewCollection[rowIndex].Parent;

			if (personFiltered.PersonPeriodCollection.Contains(personPeriod))
			{
				personFiltered.DeletePersonPeriod(personPeriod);
			}
		}

		public void DeleteSchedulePeriod(int rowIndex)
		{
			IPerson filteredPerson = _personPeriodGridViewCollection[rowIndex].Parent;

			filteredPerson.RemoveAllSchedulePeriods();
		}

		public void DeleteSchedulePeriod(int rowIndex, DateOnly selectedDate)
		{
			IPerson personFiltered = _schedulePeriodGridViewCollection[rowIndex].Parent;
			ISchedulePeriod schedulePeriod = personFiltered.SchedulePeriod(selectedDate);

			if (schedulePeriod != null)
			{
				personFiltered.RemoveSchedulePeriod(schedulePeriod);
				_schedulePeriodGridViewCollection.RemoveAt(rowIndex);
				_schedulePeriodGridViewCollection.Insert(rowIndex, new SchedulePeriodModel(selectedDate, personFiltered, _commonNameDescription));
			}
		}

		public void DeletePersonAccount(int rowIndex)
		{
			var accounts = _personAccountGridViewAdaptorCollection[rowIndex].Parent;

			IAccount account = accounts.Find(SelectedDate).FirstOrDefault();

			if (account != null)
			{
				accounts.Find(account.Owner.Absence).Remove(account);
				UpdatePersonAccounts(account);
				GetParentPersonAccountWhenUpdated(rowIndex);
			}
		}

		public void DeleteAllPersonAccounts(int rowIndex)
		{
			var accounts = _personAccountGridViewAdaptorCollection[rowIndex].Parent;
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				foreach (var account in accounts.AllPersonAccounts())
				{
					accounts.Find(account.Owner.Absence).Remove(account);
					UpdatePersonAccounts(account);
				}
			}
		}

		public void GetParentPersonAccountWhenUpdated(int rowIndex)
		{
			var accounts = _personAccountGridViewAdaptorCollection[rowIndex].Parent;
			bool canBold = _personAccountGridViewAdaptorCollection[rowIndex].CanBold;
			GridControl grid = _personAccountGridViewAdaptorCollection[rowIndex].GridControl;

			_personAccountGridViewAdaptorCollection.RemoveAt(rowIndex);

			//Fel den returnerar fler fr�ga Flaml�ndarna
			IAccount account = accounts.Find(SelectedDate).FirstOrDefault(a => a.Owner.Absence.Equals(SelectedPersonAccountAbsenceType));

			if (account != null)
			{
				//Hmm Todo: This type check is really smelly, need to fix!!!
				if ((SelectedPersonAccountAbsenceType != null) && (account.Owner.Absence != SelectedPersonAccountAbsenceType))
					account = null;
			}

			IPersonAccountModel adapter = new PersonAccountModel(RefreshService, accounts, account, _commonNameDescription);

			handleCanBold(canBold, grid, adapter);
			adapter.GridControl = grid;
			_personAccountGridViewAdaptorCollection.Insert(rowIndex, adapter);
		}

		private static void handleCanBold(bool canBold, GridControl grid, IPersonAccountModel adapter)
		{
			if (adapter.CurrentAccount != null && (adapter.CurrentAccount.Id == null || canBold))
			{
				adapter.CanBold = true;
			}
			else
			{
				ReadOnlyCollection<IPersonAccountChildModel> cachedCollection = null;

				if (grid != null)
				{
					cachedCollection = grid.Tag as
						 ReadOnlyCollection<IPersonAccountChildModel>;
				}
				adapter.CanBold = PeopleAdminHelper.IsCanBold(adapter.CurrentAccount, cachedCollection);
			}
		}

		public void DeleteSchedulePeriod(int rowIndex, ISchedulePeriod schedulePeriod)
		{
			if (schedulePeriod == null) return;

			IPerson personFiltered = _schedulePeriodGridViewCollection[rowIndex].Parent;

			if (personFiltered.PersonSchedulePeriodCollection.Contains(schedulePeriod))
			{
				personFiltered.RemoveSchedulePeriod(schedulePeriod);
			}

		}

		public void GetParentSchedulePeriods(DateOnly selectedDateTime)
		{
			_schedulePeriodGridViewCollection.Clear();
			foreach (IPerson t in _filteredPersonCollection)
			{
				var schedulePeriodModel = new SchedulePeriodModel(selectedDateTime, t, _commonNameDescription);
				_schedulePeriodGridViewCollection.Add(schedulePeriodModel);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "index-1")]
		public void UpdateOtherGridViewsWhenNewPersonIsDeleted(int index)
		{
			if (index == int.MinValue)
				throw new ArgumentOutOfRangeException("index", "input must be greater than Int32.MinValue");

			//remove from person periods
			if (_personPeriodGridViewCollection.Count >= index) _personPeriodGridViewCollection.RemoveAt(index - 1);

			// remove from schedule period
			if (_schedulePeriodGridViewCollection.Count >= index) _schedulePeriodGridViewCollection.RemoveAt(index - 1);

			//remove from person rotation grid view adapter
			if (_personRotationParentAdapterCollection.Count >= index) _personRotationParentAdapterCollection.RemoveAt(index - 1);

			//remove from the person rotation parent collection
			if (_parentPersonRotationCollection.Count >= index) _parentPersonRotationCollection.RemoveAt(index - 1);

			//remove from the person account parent collection
			if (_personAccountGridViewAdaptorCollection.Count >= index) _personAccountGridViewAdaptorCollection.RemoveAt(index - 1);

			//remove from the person availability parent collection
			if (_parentPersonAvailabilityCollection.Count >= index) _parentPersonAvailabilityCollection.RemoveAt(index - 1);
			if (_personAvailabilityParentAdapterCollection.Count >= index) _personAvailabilityParentAdapterCollection.RemoveAt(index - 1);

		}

		public void GetParentSchedulePeriodWhenUpdated(int rowIndex)
		{
			IPerson personFiltered = _schedulePeriodGridViewCollection[rowIndex].Parent;

			bool canBold = _schedulePeriodGridViewCollection[rowIndex].CanBold;
			GridControl grid = _schedulePeriodGridViewCollection[rowIndex].GridControl;

			_schedulePeriodGridViewCollection.RemoveAt(rowIndex);

			var schedulePeriodModel = new SchedulePeriodModel(SelectedDate, personFiltered, _commonNameDescription);

			ISchedulePeriod currentSchedulePeriod = schedulePeriodModel.SchedulePeriod;

			if (currentSchedulePeriod != null && (currentSchedulePeriod.Id == null || canBold))
			{
				schedulePeriodModel.CanBold = true;
			}
			else
			{
				ReadOnlyCollection<SchedulePeriodChildModel> cachedCollection = null;

				if (grid != null)
				{
					cachedCollection = grid.Tag as
						 ReadOnlyCollection<SchedulePeriodChildModel>;
				}

				schedulePeriodModel.CanBold = PeopleAdminHelper.IsCanBold(currentSchedulePeriod,
				cachedCollection);
			}

			schedulePeriodModel.GridControl = grid;

			_schedulePeriodGridViewCollection.Insert(rowIndex, schedulePeriodModel);

		}

		public void SetSelectedPeoplePeriodGridData(ReadOnlyCollection<IPersonPeriodModel> selectedPeoplePeriodGridData)
		{
			_selectedPeoplePeriodGridData = selectedPeoplePeriodGridData;
		}

		public void LoadRuleSetBag()
		{
			_ruleSetBagBindingCollection.Clear();
			var repository = new RuleSetBagRepository(GetUnitOfWork);
			var list = repository.LoadAll().Where(ptp => ptp.IsChoosable);

			IOrderedEnumerable<IRuleSetBag> sorted = list.OrderBy(n2 => n2.Description.Name);
			list = sorted.ToList();

			foreach (IRuleSetBag bag in list)
			{
				_ruleSetBagBindingCollection.Add(bag);
			}
		}

		public void LoadBudgetGroup()
		{
			_budgetGroupBindingCollection.Clear();
			var repository = new BudgetGroupRepository(GetUnitOfWork);
			var list = repository.LoadAll();

			IOrderedEnumerable<IBudgetGroup> sorted = list.OrderBy(n2 => n2.Name);
			list = sorted.ToList();

			_budgetGroupBindingCollection.Add(PersonPeriodModel.NullBudgetGroup);
			foreach (IBudgetGroup budgetGroup in list)
			{
				_budgetGroupBindingCollection.Add(budgetGroup);
			}
		}

		public void MarkForRemove(IPerson person)
		{
			person.AuthenticationInfo = null;
			person.ApplicationAuthenticationInfo = null;
			new Repository(GetUnitOfWork).Remove(person);

			toBeRemovedList.Add(person.Id.GetValueOrDefault());
		}

		public void MarkForInsert(IAggregateRoot person)
		{
			new Repository(GetUnitOfWork).Add(person);
		}

		public void DeleteAndSavePerson(IPerson person)
		{
			InParameter.NotNull("Person p", person);
			//Remove tobe deleted person from necessary collections.
			_personCollection.Remove(person);
			_filteredPersonCollection.Remove(person);
			_validateUserCredentialsCollection.Remove(person);

			MarkForRemove(person);
		}

		private void LoadPersonSkills()
		{
			_personSkillCollection.Clear();
			_personSkillAdapterCollection.Clear();

			ISkillRepository rep = new SkillRepository(GetUnitOfWork);

			ICollection<ISkill> skillCollection = rep.FindAllWithoutMultisiteSkills();

			foreach (ISkill skill in skillCollection)
			{
				_personSkillCollection.Add(new PersonSkill(skill, new Percent(1)));
			}

			_personSkillAdapterCollection.AddRange(_personSkillCollection.ConvertAll(
						(EntityConverter.ConvertToOther<IPersonSkill, PersonSkillModel>)));
		}

		private void LoadExternalLogOn()
		{
			_externalLogOnCollection.Clear();

			var r = new ExternalLogOnRepository(GetUnitOfWork);
			var logObjectUniqueRecord = new QueueSourceRepository(GetUnitOfWork);
			var logObjectInfo = logObjectUniqueRecord.GetDistinctLogItemName();
			var externalLogOnList = r.LoadAllExternalLogOns();
			foreach (var extLogOnItem in externalLogOnList)
			{
				if (logObjectInfo.ContainsKey(extLogOnItem.DataSourceId))
				{
					extLogOnItem.DataSourceName = logObjectInfo[extLogOnItem.DataSourceId];
				}

			}
			_externalLogOnCollection.AddRange(externalLogOnList);
			_externalLogOnCollection.Sort();
			_externalLogOnAdapterCollection.AddRange(_externalLogOnCollection.ConvertAll((EntityConverter.ConvertToOther<IExternalLogOn,
				 ExternalLogOnModel>)));
		}

		public void SetPersonExternalLogOnByPersonPeriod(ReadOnlyCollection<IPersonPeriod> personPeriods,
			 ReadOnlyCollection<IPersonPeriodModel> personPeriodGridData)
		{
			SetSelectedPeoplePeriodGridData(personPeriodGridData);
			ResetExternalLogOnAdapterCollection();

			//Set person skill view data
			foreach (IPersonPeriod personPeriod in personPeriods)
			{
				foreach (ExternalLogOnModel externalLogOnModel in _externalLogOnAdapterCollection)
				{
					if (personPeriod == null) continue;
					ExternalLogOnModel model = externalLogOnModel;
					IExternalLogOn externalLogOn = personPeriod.ExternalLogOnCollection.FirstOrDefault(s => s.Equals(model.ContainedEntity));
					if (externalLogOn != null)
					{
						externalLogOnModel.ExternalLogOnInPersonCount += 1;

						externalLogOnModel.TriState = externalLogOnModel.ExternalLogOnInPersonCount == personPeriods.Count ? 1 : 2;
					}
				}
			}
		}

		public void ResetExternalLogOnAdapterCollection()
		{
			foreach (ExternalLogOnModel externalLogOnModel in _externalLogOnAdapterCollection)
			{
				externalLogOnModel.ExternalLogOnInPersonCount = 0;
				externalLogOnModel.TriState = 0;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Double.ToString")]
		public void SetPersonSkillGridViewDataByPersons(ReadOnlyCollection<IPersonPeriod> personPeriods,
				ReadOnlyCollection<IPersonPeriodModel> personPeriodGridData)
		{
			SetSelectedPeoplePeriodGridData(personPeriodGridData);
			//Reset person skill view data
			ResetPersonSkillAdapterCollection();

			//Set person skill view data
			foreach (var personPeriod in personPeriods)
			{

				foreach (var skillModel in _personSkillAdapterCollection)
				{
					var model = skillModel;
					IList<IPersonSkill> personSkills;
					personSkills = personPeriod == null ? new List<IPersonSkill>() : personPeriod.PersonSkillCollection.Where(s => s.Skill.Equals(model.ContainedEntity.Skill)).ToList();

					if (personSkills.Count <= 0)
					{
						skillModel.AddProficiencyValue("100");
						skillModel.ActiveTriState = 0;
						//continue;
					}
					else
					{
						skillModel.PersonSkillExistsInPersonCount += 1;
						skillModel.TriState = skillModel.PersonSkillExistsInPersonCount == personPeriods.Count ? 1 : 2;
					}

					foreach (var personSkill in personSkills)
					{
						skillModel.AddProficiencyValue((personSkill.SkillPercentage.Value * 100).ToString());
					}

					var activeSkills = personSkills.Where(personSkill => personSkill.Active).ToList();
					skillModel.ActiveSkillsInPersonPeriodCount += activeSkills.Count;

					if (skillModel.ActiveSkillsInPersonPeriodCount == 0)
						skillModel.ActiveTriState = 0;
					else
					{
						skillModel.ActiveTriState = skillModel.ActiveSkillsInPersonPeriodCount == personPeriods.Count ? 1 : 2;
					}
				}
			}
		}

		public void ResetPersonSkillAdapterCollection()
		{
			foreach (PersonSkillModel personSkillModel in _personSkillAdapterCollection)
			{
				personSkillModel.PersonSkillExistsInPersonCount = 0;
				personSkillModel.ActiveSkillsInPersonPeriodCount = 0;
				personSkillModel.TriState = 0;
				personSkillModel.Proficiency = 100;
				personSkillModel.ProficiencyValues = new StringCollection();
			}
		}

		private void LoadAllOptionalColumns()
		{
			_optionalColumnCollection.Clear();

			var rep = new OptionalColumnRepository(GetUnitOfWork);
			_optionalColumnCollection = rep.GetOptionalColumns<Person>();
		}

		public void LoadIt()
		{
			LoadSettings();

			LoadPersonSkills();
			LoadExternalLogOn();

			LoadAllApplicationRoles();
			LoadAllOptionalColumns();
			LoadTeams();

			LoadAllAbsence();
			GetAbsenceForPersonAccounts();
		}


		private void LoadAllApplicationRoles()
		{
			_applicationRoleCollection.Clear();

			var r = new ApplicationRoleRepository(GetUnitOfWork);
			_applicationRoleCollection.AddRange(r.LoadAllApplicationRolesSortedByName());

			_rolesViewAdapterCollection.AddRange(_applicationRoleCollection.ConvertAll((EntityConverter.ConvertToOther<IApplicationRole, RolesModel>)));
		}

		public void ResetRolesViewAdapterCollection()
		{
			foreach (RolesModel t in _rolesViewAdapterCollection)
			{
				t.RoleExistsInPersonCount = 0;
				t.TriState = 0;
			}
		}

		public void SetRoleGridViewDataByPersons(ReadOnlyCollection<IPerson> persons, ReadOnlyCollection<PersonGeneralModel> peopleGridData)
		{
			setSelectedPeopleGeneralGridData(peopleGridData);

			//reset roles view data
			ResetRolesViewAdapterCollection();

			//set roles view data
			foreach (IPerson person in persons)
			{
				foreach (RolesModel rolesModel in _rolesViewAdapterCollection)
				{
					if (person.PermissionInformation.ApplicationRoleCollection.Contains(rolesModel.Role))
					{
						rolesModel.RoleExistsInPersonCount += 1;
						rolesModel.TriState = rolesModel.RoleExistsInPersonCount == persons.Count ? 1 : 2;
					}
				}
			}
		}

		private void setSelectedPeopleGeneralGridData(ReadOnlyCollection<PersonGeneralModel> peopleGridData)
		{
			_selectedPeopleGeneralGridData = peopleGridData;
		}

		public void SetRoleForSelectedPersons(IApplicationRole appRole)
		{
			if (_selectedPeopleGeneralGridData == null) return;
			foreach (PersonGeneralModel generalModel in _selectedPeopleGeneralGridData)
			{
				if (!generalModel.ContainedEntity.PermissionInformation.ApplicationRoleCollection.Contains(appRole))
				{
					generalModel.ContainedEntity.PermissionInformation.AddApplicationRole(appRole);
					generalModel.CanBold = true;
				}
			}
		}

		public void RemoveRoleForSelectedPersons(IApplicationRole appRole)
		{
			if (_selectedPeopleGeneralGridData == null) return;
			foreach (PersonGeneralModel generalModel in _selectedPeopleGeneralGridData)
			{
				if (generalModel.ContainedEntity.PermissionInformation.ApplicationRoleCollection.Contains(appRole))
				{
					generalModel.ContainedEntity.PermissionInformation.RemoveApplicationRole(appRole);
					generalModel.CanBold = true;
				}
			}
		}

		public ReadOnlyCollection<IApplicationRole> ApplicationRoleCollection
		{
			get { return new ReadOnlyCollection<IApplicationRole>(_applicationRoleCollection); }
		}

		public ReadOnlyCollection<RolesModel> RolesViewAdapterCollection
		{
			get { return new ReadOnlyCollection<RolesModel>(_rolesViewAdapterCollection); }
		}

		private void LoadSettings()
		{
			IUnitOfWork uow = UnitOfWorkFactory.CurrentUnitOfWork().Current();
			ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);
			_commonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
		}

		public void ResetBoldProperty()
		{
			resetBoldPropertyFromPeopleAdminGridAdaperCollection();
			resetBoldPropertyFromPersonPeriodGridViewAdapterCollection();
			resetBoldPropertyFromRotationGridViewAdapterCollection();
			resetBoldPropertyFromSchedulePeriodGridViewAdapterCollection();
			resetBoldPropertyFromPersonAccountAdapterCollection();
		}

		private void resetBoldPropertyFromPersonAccountAdapterCollection()
		{
			foreach (IPersonAccountModel adapter in _personAccountGridViewAdaptorCollection)
			{
				adapter.CanBold = false;
				adapter.ResetCanBoldPropertyOfChildAdapters();
			}
		}

		private void resetBoldPropertyFromSchedulePeriodGridViewAdapterCollection()
		{
			foreach (SchedulePeriodModel adapter in _schedulePeriodGridViewCollection)
			{
				adapter.CanBold = false;
				adapter.ResetCanBoldPropertyOfChildAdapters();
			}
		}

		private void resetBoldPropertyFromRotationGridViewAdapterCollection()
		{
			foreach (PersonRotationModelParent adapter in _personRotationParentAdapterCollection)
			{
				adapter.CanBold = false;
				adapter.ResetCanBoldPropertyOfChildAdapters();
			}

			foreach (PersonAvailabilityModelParent adapter in _personAvailabilityParentAdapterCollection)
			{
				adapter.CanBold = false;
				adapter.ResetCanBoldPropertyOfChildAdapters();
			}

		}

		private void resetBoldPropertyFromPersonPeriodGridViewAdapterCollection()
		{
			foreach (PersonPeriodModel adapter in _personPeriodGridViewCollection)
			{
				adapter.CanBold = false;
				adapter.ResetCanBoldPropertyOfChildAdapters();
			}
		}

		private void resetBoldPropertyFromPeopleAdminGridAdaperCollection()
		{
			foreach (PersonGeneralModel adapter in FilteredPeopleGridData)
			{
				adapter.CanBold = false;
			}
		}

		public IList<ISameUserCredentialOnOther> CheckForDuplicateUserNames()
		{
			var retList = new List<ISameUserCredentialOnOther>();
			if (_validateUserCredentialsCollection.Count > 0)
			{
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var rep = new PersonRepository(uow);
					var sameUserCredentialOnOtherChecker =
						 new SameUserCredentialOnOtherChecker(rep);
					retList.AddRange(
						 sameUserCredentialOnOtherChecker.CheckConflictsBeforeSave(_validateUserCredentialsCollection));
				}
			}
			return retList;
		}

		public bool CheckBadPasswordPolicy()
		{
			return ValidatePasswordPolicy.Count <= 0;
		}

		public void AddRootsToRepository()
		{
			if (_newPersonRotationCollection.Count > 0)
			{
				var personRotationRepository = new PersonRotationRepository(GetUnitOfWork);

				foreach (IPersonRotation rotation in _newPersonRotationCollection)
				{
					personRotationRepository.Add(rotation);
				}
			}

			if (_newPersonAvailabilityCollection.Count > 0)
			{
				var personAvailabilityRepository = new PersonAvailabilityRepository(GetUnitOfWork);

				foreach (IPersonAvailability availability in _newPersonAvailabilityCollection)
				{
					personAvailabilityRepository.Add(availability);
				}
			}


			var rep = new PersonAbsenceAccountRepository(GetUnitOfWork);
			foreach (var account in AllAccounts)
			{
				foreach (var accountCollection in account.Value)
				{
					rep.Add(accountCollection);
				}
			}

			_newPersonRotationCollection.Clear();
			_newPersonAvailabilityCollection.Clear();
		}

		private void LoadAllAbsence()
		{
			_absenceCollection.Clear();

			var absenceRepository = new AbsenceRepository(GetUnitOfWork);
			_absenceCollection.AddRange(absenceRepository.LoadAllSortByName());
		}

		private void GetAbsenceForPersonAccounts()
		{
			if ((_absenceCollection != null) && (_absenceCollection.Count > 0))
			{
				// Filteres the absences which have the trackers - For person Account we need only 
				// the absence which have trackers. 
				// Since the tracker is the key to indentify the person account type (day or time).
				_filteredAbsenceCollection = (from absence in _absenceCollection
														where absence.Tracker != null
														select absence).ToList();
			}
		}


		private void getParentPersonPeriods(IPerson person, DateOnly selectedDateTime)
		{
			var personPeriodModel = new PersonPeriodModel(selectedDateTime,
																		person,
																		_personSkillCollection,
																		_externalLogOnCollection,
																		_siteTeamBindingCollection, _commonNameDescription);
			_personPeriodGridViewCollection.Add(personPeriodModel);

		}

		private void GetParentSchedulePeriods(IPerson person, DateOnly selectedDateTime)
		{
			var schedulePeriodModel = new SchedulePeriodModel(selectedDateTime, person, _commonNameDescription);
			_schedulePeriodGridViewCollection.Add(schedulePeriodModel);
		}

		public IPersonRotation GetCurrentPersonRotation(IList<IPersonRotation> personRotationCollection)
		{
			//the person rotation collection is already sorted in the descending order
			if (personRotationCollection.Count > 0)
			{
				//if the today is larger than the largest item in the collection 
				//or smaller than the smallest item in the collection, there is no point in having the current item.
				//therefore null is returned.
				if (personRotationCollection[personRotationCollection.Count - 1].StartDate > SelectedDate)
				{
					return null;
				}

				//since the collection is sorted in the decsendign order, the first item that is found greater 
				//than today, should be the current person rotation.
				foreach (IPersonRotation rotation in personRotationCollection)
				{
					if (rotation.StartDate <= SelectedDate)
						return rotation;
				}
				return null;
			}
			return null;
		}

		public IPersonAvailability GetCurrentPersonAvailability(IList<IPersonAvailability> personAvailabilityCollection)
		{
			//the person rotation collection is already sorted in the descending order
			if (personAvailabilityCollection.Count > 0)
			{
				//if the today is larger than the largest item in the collection 
				//or smaller than the smallest item in the collection, there is no point in having the current item.
				//therefore null is returned.
				if (personAvailabilityCollection[personAvailabilityCollection.Count - 1].StartDate > SelectedDate)
				{
					return null;
				}
				//since the collection is sorted in the decsendign order, the first item that is found greater 
				//than today, should be the current person rotation.
				foreach (IPersonAvailability availability in personAvailabilityCollection)
				{
					if (availability.StartDate <= SelectedDate)
						return availability;
				}
				return null;
			}
			return null;
		}

		public IPersonRotation GetSamplePersonRotation(IPerson person, DateOnly selectedDateTime)
		{
			IRotation rotation = GetDefaultRotation;

			var selectedDate = new DateOnly(PeopleAdminHelper.GetFirstDayOfWeek(selectedDateTime.Date));

			if (rotation != null)
				return new PersonRotation(person, rotation, selectedDate, 0);

			return null;
		}

		private void UpdatePersonAccounts(IAccount account)
		{
			var updater = new FilteredPeopleAccountUpdater(this, UnitOfWorkFactory.Current);
			updater.Update(account.Owner.Person);
		}

		public IRotation GetDefaultRotation
		{
			get
			{
				if (_rotationCollection != null && _rotationCollection.Count > 0)
				{
					return _rotationCollection[0];
				}

				return null;
			}
		}

		public IAvailabilityRotation GetDefaultAvailability
		{
			get
			{
				if (_availabilityCollection != null && _availabilityCollection.Count > 0)
				{
					return _availabilityCollection[0];
				}
				return null;
			}
		}

		public IDictionary<IPerson, IPersonAccountCollection> AllAccounts
		{
			get { return _allAccounts; }
		}

		public IList<PersonGeneralModel> ValidatePasswordPolicy
		{
			get { return _validatePasswordPolicy; }
		}

		public ITraceableRefreshService RefreshService
		{
			get { return _refreshService; }
		}

		private void GetParentPersonAccounts(IPerson person, DateOnly selectedDate)
		{
			//fel h�r. kan vara flera. fr�ga estl�ndarna.
			IAccount account = AllAccounts[person].Find(selectedDate).FirstOrDefault();

			// Gets the person account adoptor using the person data and the selcted date
			IPersonAccountModel personAccountModel = new PersonAccountModel(RefreshService, AllAccounts[person], account, _commonNameDescription);
			_personAccountGridViewAdaptorCollection.Add(personAccountModel);
		}

		public void GetParentPersonAccounts()
		{
			if (_personAccountGridViewAdaptorCollection.Count > 0) _personAccountGridViewAdaptorCollection.Clear();

			foreach (IPerson person in _filteredPersonCollection)
			{
				//fel h�r. kan vara flera. fr�ga sydkoreanerna
				IAccount account = AllAccounts[person].Find(SelectedDate).FirstOrDefault();
				// Gets the person account adoptor using the person data and the selcted date
				IPersonAccountModel personAccountModel = new PersonAccountModel(RefreshService, AllAccounts[person], account, _commonNameDescription);
				_personAccountGridViewAdaptorCollection.Add(personAccountModel);
			}
		}

		public void GetFilteredParentPersonAccounts()
		{
			// Clears the older data collection
			_personAccountGridViewAdaptorCollection.Clear();

			if (SelectedPersonAccountAbsenceType != null)
			{
				filterPersonAccounts(SelectedPersonAccountAbsenceType);
			}
			else
			{
				GetParentPersonAccounts();
			}
		}

		public void GetParentPersonPeriods()
		{
			_personPeriodGridViewCollection.Clear();
			foreach (IPerson person in _filteredPersonCollection)
			{
				var personPeriodModel = new PersonPeriodModel(SelectedDate,
																			 person,
																			 _personSkillCollection,
																			 _externalLogOnCollection,
																			 _siteTeamBindingCollection,
																			 _commonNameDescription);
				_personPeriodGridViewCollection.Add(personPeriodModel);
			}
		}

		public void GetParentPersonPeriodWhenUpdated(int rowIndex)
		{
			IPerson filteredPerson = _personPeriodGridViewCollection[rowIndex].Parent;
			bool canBold = _personPeriodGridViewCollection[rowIndex].CanBold;
			GridControl grid = _personPeriodGridViewCollection[rowIndex].GridControl;

			_personPeriodGridViewCollection.RemoveAt(rowIndex);


			var personPeriodModel = new PersonPeriodModel(SelectedDate,
																							filteredPerson,
																							_personSkillCollection,
																							_externalLogOnCollection,
																							_siteTeamBindingCollection,
																							_commonNameDescription);
			IPersonPeriod currentPersonPeriod = personPeriodModel.Period;



			if (currentPersonPeriod != null && (currentPersonPeriod.Id == null || canBold))
			{
				personPeriodModel.CanBold = true;
			}
			else
			{
				ReadOnlyCollection<PersonPeriodChildModel> cachedCollection = null;

				if (grid != null)
				{
					cachedCollection = grid.Tag as
						 ReadOnlyCollection<PersonPeriodChildModel>;
				}

				personPeriodModel.CanBold = PeopleAdminHelper.IsCanBold(currentPersonPeriod,
				cachedCollection);

			}

			personPeriodModel.GridControl = grid;
			_personPeriodGridViewCollection.Insert(rowIndex, personPeriodModel);
		}

		private void filterPersonAccounts(IAbsence selectedType)
		{
			foreach (IPerson person in _filteredPersonCollection)
			{
				IAccount account = AllAccounts[person].Find(selectedType, SelectedDate);
				if (account != null)
				{
					if ((account.Owner.Absence != null) && (account.Owner.Absence != selectedType))
						account = null;
				}

				// Gets the person account adoptor using the person data and the selcted date
				IPersonAccountModel personAccountModel = new PersonAccountModel(RefreshService, AllAccounts[person], account, _commonNameDescription);
				_personAccountGridViewAdaptorCollection.Add(personAccountModel);
			}
		}

		public void SetSortedPeopleFilteredList(IList<PersonGeneralModel> result)
		{
			_filteredPeopleGridData = result.ToList();
		}

		public void SetSortedPersonPeriodFilteredList(IList<PersonPeriodModel> result)
		{
			_personPeriodGridViewCollection = result.ToList();
		}

		public void SetSortedSchedulePeriodFilteredList(IList<SchedulePeriodModel> result)
		{
			_schedulePeriodGridViewCollection = result.ToList();
		}

		public void PersistAll()
		{

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				GetUnitOfWork.PersistAll();
				uow.PersistAll();
			}
			PersistTenantData();
		}

		public void PersistTenantData()
		{
			var changed = (from personGeneralModel in FilteredPeopleGridData
								where personGeneralModel.TenantData.Changed
								select personGeneralModel.TenantData).ToList();

			if (changed.Any())
			{
				foreach (var tenantAuthenticationData in changed)
				{
					var result = _tenantDataManager.SaveTenantData(tenantAuthenticationData);
					if (!result.Success)
					{
						MessageBox.Show(result.FailReason, UserTexts.Resources.SaveError);
						return;
					}

				}
			}

			//reset after save
			foreach (var personGeneralModel in FilteredPeopleGridData)
			{
				personGeneralModel.TenantData.Changed = false;
			}
			if (toBeRemovedList.Any())
				_tenantDataManager.DeleteTenantPersons(toBeRemovedList);

			toBeRemovedList.Clear();
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (GetUnitOfWork != null) GetUnitOfWork.Dispose();
			}
		}
		#endregion
	}
}
