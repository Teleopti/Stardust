using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ViewType = Teleopti.Ccc.Win.PeopleAdmin.Views.ViewType;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
    public class WorksheetStateHolder : IDisposable
    {
        private readonly IDictionary<ViewType, IRotationStateHolder> _rotationStateHolderCache = new Dictionary<ViewType, IRotationStateHolder>();

        private IList<IShiftCategory> _shiftCategories = new List<IShiftCategory>();
		private readonly TypedBindingCollection<IContractSchedule> _contractScheduleBindingCollection = new TypedBindingCollection<IContractSchedule>();

		private readonly TypedBindingCollection<IPartTimePercentage> _partTimePercentageBindingCollection = new TypedBindingCollection<IPartTimePercentage>();

		private readonly List<PersonGeneralModel> _tobeDeleteFromGridDataAfterRomove = new List<PersonGeneralModel>();

		private readonly TypedBindingCollection<IContract> _contractBindingCollection = new TypedBindingCollection<IContract>();
		private readonly List<Culture> _cultureCollection = new List<Culture>();
		private readonly List<Culture> _uiCultureCollection = new List<Culture>();
		
		private List<PersonPeriodChildModel> _personPeriodGridViewChildCollection = new List<PersonPeriodChildModel>();
	    private List<SchedulePeriodChildModel> _schedulePeriodGridViewChildCollection;
		
		private IList<PersonRotationModelChild> _personRotationChildAdapterCollection;
		private IList<PersonAvailabilityModelChild> _personAvailablityChildAdapterCollection;
		
		private IList<IPersonRotation> _childrenPersonRotationCollection = new List<IPersonRotation>();
		private IList<IPersonAvailability> _childrenPersonAvailabilityCollection = new List<IPersonAvailability>();
		private TypedBindingCollection<IRotation> _rotationCollection = new TypedBindingCollection<IRotation>();
		private readonly TypedBindingCollection<IAvailabilityRotation> _availabilityCollection = new TypedBindingCollection<IAvailabilityRotation>();
		private List<IPersonAccountChildModel> _personaccountGridViewChildCollection;

	    private readonly TypedBindingCollection<IWorkflowControlSet> _workflowControlSetCollection = new TypedBindingCollection<IWorkflowControlSet>();

        public TypedBindingCollection<IContract> ContractCollection
        {
            get { return _contractBindingCollection; }
        }

        public TypedBindingCollection<IContractSchedule> ContractScheduleCollection
        {
            get { return _contractScheduleBindingCollection; }
        }

        public TypedBindingCollection<IPartTimePercentage> PartTimePercentageCollection
        {
            get { return _partTimePercentageBindingCollection; }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rotationCollection = null;
            }
        }

        public ReadOnlyCollection<Culture> CultureCollection
        {
            get { return new ReadOnlyCollection<Culture>(_cultureCollection); }
        }

        public ReadOnlyCollection<Culture> UICultureCollection
        {
            get { return new ReadOnlyCollection<Culture>(_uiCultureCollection); }
        }

        public void LoadCultureInfo()
        {
            Culture culture;
            Culture uiCulture;

            CultureInfo[] cInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            for (int i = 0; i < cInfo.Length - 1; i++)
            {
                culture = new Culture(cInfo[i].LCID, cInfo[i].DisplayName);
                _cultureCollection.Add(culture);

                uiCulture = new Culture(cInfo[i].LCID, cInfo[i].Name);
                _uiCultureCollection.Add(uiCulture);
            }

            _cultureCollection.Sort(
                (c1, c2) => string.Compare(c1.DisplayName, c2.DisplayName, StringComparison.CurrentCulture));
            _uiCultureCollection.Sort(
                (c1, c2) => string.Compare(c1.DisplayName, c2.DisplayName, StringComparison.CurrentCulture));

            culture = new Culture(0, UserTexts.Resources.ChangeYourCultureSettings);
            _cultureCollection.Insert(0, culture);
            uiCulture = new Culture(0, UserTexts.Resources.ChangeYourCultureSettings);
            _uiCultureCollection.Insert(0, uiCulture);
        }

        internal void AddAndSavePerson(int rowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPerson newPerson = new Person
                                	{
                                		Name =
                                			new Name("<" + UserTexts.Resources.FirstName + ">",
                                			         "<" + UserTexts.Resources.LastName + ">")
                                	};
        	newPerson.PermissionInformation.SetDefaultTimeZone(
                StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);
            //filteredPeopleHolder.PersonCollection.Insert(rowIndex, newPerson);
            filteredPeopleHolder.FilteredPersonCollection.Insert(rowIndex, newPerson);

            filteredPeopleHolder.MarkForInsert(newPerson);

			var gridData = new PersonGeneralModel(newPerson, new UserDetail(newPerson), new PrincipalAuthorization(new CurrentTeleoptiPrincipal()), new FilteredPeopleAccountUpdater(filteredPeopleHolder,  UnitOfWorkFactory.Current)) { CanBold = true };

        	gridData.SetAvailableRoles(filteredPeopleHolder.ApplicationRoleCollection);

            //set optional columns if any.);
            if (filteredPeopleHolder.OptionalColumnCollection.Count > 0)
                gridData.SetOptionalColumns(filteredPeopleHolder.OptionalColumnCollection);

            filteredPeopleHolder.FilteredPeopleGridData.Insert(rowIndex, gridData);

            //Set other collections accordingly.
            updateOtherGridViewsWhenANewPersonIsAdded(newPerson, filteredPeopleHolder);
        }

        internal void DeletePersonByIndex(int index, FilteredPeopleHolder filteredPeopleHolder)
        {
            if (index > 0)
            {
                IPerson personTobeDelete = filteredPeopleHolder.FilteredPeopleGridData[index - 1].ContainedEntity;
                filteredPeopleHolder.DeleteAndSavePerson(personTobeDelete);

                _tobeDeleteFromGridDataAfterRomove.Add(filteredPeopleHolder.FilteredPeopleGridData[index - 1]);

                filteredPeopleHolder.UpdateOtherGridViewsWhenNewPersonIsDeleted(index);
            }
        }

        internal void TobeDeleteFromGridDataAfterRomove(FilteredPeopleHolder filteredPeopleHolder)
        {
            int tobeRemoveCount = _tobeDeleteFromGridDataAfterRomove.Count;
            for (int i = 0; i < tobeRemoveCount; i++)
            {
                filteredPeopleHolder.FilteredPeopleGridData.Remove(_tobeDeleteFromGridDataAfterRomove[i]);
            }
            _tobeDeleteFromGridDataAfterRomove.Clear();
        }

        internal void InsertFromClipHandler(int rowIndex, ClipHandler<string> clipHandler, FilteredPeopleHolder filteredPeopleHolder)
        {
            var persons = new List<IPerson>();
            for (int row = 0; row < clipHandler.RowSpan(); row++)
            {
                IPerson person = new Person();
                person.PermissionInformation.SetDefaultTimeZone(
                    StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);
                persons.Add(person);

                //filteredPeopleHolder.PersonCollection.Insert(rowIndex, person);
                filteredPeopleHolder.FilteredPersonCollection.Insert(rowIndex, person);

                filteredPeopleHolder.MarkForInsert(person);

				var personGridData = new PersonGeneralModel(person, new UserDetail(person), new PrincipalAuthorization(new CurrentTeleoptiPrincipal()), new FilteredPeopleAccountUpdater(filteredPeopleHolder, UnitOfWorkFactory.Current)) { CanBold = true };
            	personGridData.SetAvailableRoles(filteredPeopleHolder.ApplicationRoleCollection);

                //set optional columns if any.);
                if (filteredPeopleHolder.OptionalColumnCollection.Count > 0)
                    personGridData.SetOptionalColumns(filteredPeopleHolder.OptionalColumnCollection);
                //add personGridData into grid data collection.
                filteredPeopleHolder.FilteredPeopleGridData.Insert(rowIndex, personGridData);

                rowIndex += 1;
            }

            //Add person periods for newly added persons
            foreach (IPerson t in persons)
            {
				//Set other collections accordingly.
            	updateOtherGridViewsWhenANewPersonIsAdded(t, filteredPeopleHolder);
            }
        }

        private void updateOtherGridViewsWhenANewPersonIsAdded(IPerson newPerson, FilteredPeopleHolder filteredPeopleHolder)
        {
            //Add person period for newly add person
            var personPeriodModel = new PersonPeriodModel(DateOnly.Today, 
                newPerson,
                filteredPeopleHolder.PersonSkillCollection, filteredPeopleHolder.ExternalLogOnCollection,
                filteredPeopleHolder.SiteTeamAdapterCollection,filteredPeopleHolder.CommonNameDescription);
            filteredPeopleHolder.PersonPeriodGridViewCollection.Add(personPeriodModel);

            //Add person period for newly add person
            var schedulePeriodModel = new SchedulePeriodModel(DateOnly.Today, newPerson, filteredPeopleHolder.CommonNameDescription);
            filteredPeopleHolder.SchedulePeriodGridViewCollection.Add(schedulePeriodModel);

            IPersonRotation newPersonRotation = GetSamplePersonRotation(newPerson);
            filteredPeopleHolder.ParentPersonRotationCollection.Add(newPersonRotation);

            //Add person rotation parent item for the newly added item
            var parentRotationItem = new PersonRotationModelParent(newPerson,filteredPeopleHolder.CommonNameDescription);
            filteredPeopleHolder.PersonRotationParentAdapterCollection.Add(parentRotationItem);

            // Add person account acdopter for the newly added item
            filteredPeopleHolder.PersonAccountModelCollection.Add(new PersonAccountModel(filteredPeopleHolder.RefreshService,filteredPeopleHolder.AllAccounts[newPerson], null,filteredPeopleHolder.CommonNameDescription));

            if (AllAvailabilities.Count > 0)
            {
                IPersonAvailability newPersonAvailability = GetSamplePersonAvailability(newPerson);
                filteredPeopleHolder.ParentPersonAvailabilityCollection.Add(newPersonAvailability);

                var parentAvailabilityItem =
                    new PersonAvailabilityModelParent(newPerson, null,filteredPeopleHolder.CommonNameDescription);
                filteredPeopleHolder.PersonAvailabilityParentAdapterCollection.Add(parentAvailabilityItem);
            }
        }

        

        public ReadOnlyCollection<PersonPeriodChildModel> PersonPeriodChildGridData
        {
            get
            {
                _personPeriodGridViewChildCollection = _personPeriodGridViewChildCollection.OrderByDescending(n2 => n2.PeriodDate).ToList();

                return new ReadOnlyCollection<PersonPeriodChildModel>(_personPeriodGridViewChildCollection);
            }
        }

        public ReadOnlyCollection<PersonPeriodChildModel> PersonPeriodChildGridDataWhenAddChild
        {
            get
            {
                //Get Last Item 
                PersonPeriodChildModel model =
                    _personPeriodGridViewChildCollection[_personPeriodGridViewChildCollection.Count - 1];

                _personPeriodGridViewChildCollection.Remove(model);

                _personPeriodGridViewChildCollection = _personPeriodGridViewChildCollection.OrderByDescending(n2 => n2.PeriodDate).ToList();

                _personPeriodGridViewChildCollection.Insert(0, model);


                return new ReadOnlyCollection<PersonPeriodChildModel>(_personPeriodGridViewChildCollection);
            }
        }

        public string CurrentChildName
        {
            get;
            set;
        }

        public void GetChildPersonPeriods(int index, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPerson filteredPerson = filteredPeopleHolder.PersonPeriodGridViewCollection[index].Parent;

            bool canBold = filteredPeopleHolder.PersonPeriodGridViewCollection[index].CanBold;
            IPersonPeriod currentPersonPeriod = filteredPeopleHolder.PersonPeriodGridViewCollection[index].Period;

            IList<IPersonPeriod> personPeriodCollection = new List<IPersonPeriod>(filteredPerson.PersonPeriodCollection);

            _personPeriodGridViewChildCollection = new List<PersonPeriodChildModel>();
            CurrentChildName = filteredPeopleHolder.CommonNameDescription.BuildCommonNameDescription(filteredPerson); //.Name.ToString();

            foreach (IPersonPeriod personPeriod in personPeriodCollection)
            {
            	PersonPeriodChildModel model =
            		EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(personPeriod);

            	if (((personPeriod == currentPersonPeriod) && canBold) || personPeriod.Id == null)
            	{
            		model.CanBold = true;
            	}

            	model.SetPersonSkillCollection(filteredPeopleHolder.PersonSkillCollection);
            	model.SetPersonExternalLogOnCollection(filteredPeopleHolder.ExternalLogOnCollection);
            	model.SetSiteTeamAdapterCollection(filteredPeopleHolder.SiteTeamAdapterCollection);

            	model.FullName =filteredPeopleHolder.CommonNameDescription.BuildCommonNameDescription(filteredPerson); // .Name.ToString();
            	_personPeriodGridViewChildCollection.Add(model);
            }
        }

        public void GetChildPersonPeriods(int index, FilteredPeopleHolder filteredPeopleHolder,
            ReadOnlyCollection<PersonPeriodChildModel> cachedPersonPeriodChildCollection)
        {
            IPerson filteredPerson = filteredPeopleHolder.PersonPeriodGridViewCollection[index].Parent;
            bool canBold = filteredPeopleHolder.PersonPeriodGridViewCollection[index].CanBold;
            IPersonPeriod currentPersonPeriod = filteredPeopleHolder.PersonPeriodGridViewCollection[index].Period;

            IList<IPersonPeriod> personPeriodCollection = new List<IPersonPeriod>(filteredPerson.PersonPeriodCollection);

            _personPeriodGridViewChildCollection = new List<PersonPeriodChildModel>();
            CurrentChildName =filteredPeopleHolder.CommonNameDescription.BuildCommonNameDescription( filteredPerson); //.Name.ToString();

            foreach (IPersonPeriod personPeriod in personPeriodCollection)
            {
            	PersonPeriodChildModel model =
            		EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(personPeriod);

            	if (personPeriod != null && personPeriod.Id == null)
            	{
            		model.CanBold = true;
            	}
            	else
            	{
            		model.CanBold = PeopleAdminHelper.IsCanBold(personPeriod, cachedPersonPeriodChildCollection);
            	}

            	if (personPeriod == currentPersonPeriod)
            	{
            		// This is fixed for following secnario :If user click save button and user is changing current 
            		// period in the grid then he expands the grid changes still should be bold.
            		// (Apply only when model.Canbold is false)

            		if (!model.CanBold)
            		{
            			model.CanBold = canBold;
            		}
            	}
            	model.SetPersonSkillCollection(filteredPeopleHolder.PersonSkillCollection);
            	model.SetPersonExternalLogOnCollection(filteredPeopleHolder.ExternalLogOnCollection);
            	model.SetSiteTeamAdapterCollection(filteredPeopleHolder.SiteTeamAdapterCollection);

            	model.FullName = filteredPerson.Name.ToString();
            	_personPeriodGridViewChildCollection.Add(model);
            }
        }

        public IPersonPeriod AddPersonPeriod(int rowIndex, Collection<PersonPeriodModel>
            personPeriodGridViewCollection, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPerson personFiltered = personPeriodGridViewCollection[rowIndex].Parent;
            IPersonPeriod personPeriod = getSamplePersonPeriod(filteredPeopleHolder, 
                personPeriodGridViewCollection[rowIndex].Period, personFiltered);

            if (personPeriod != null)
            {
                personFiltered.AddPersonPeriod(personPeriod);
            }

            return personPeriod;
        }

        private IPersonPeriod getSamplePersonPeriod(FilteredPeopleHolder filteredHolder, IPersonPeriod currentPersonPeriod, IPerson person)
        {
            IPersonPeriod personPeriod;
            var date = DateOnly.Today;
            
            if(currentPersonPeriod != null)
            {
                personPeriod = currentPersonPeriod.Clone() as IPersonPeriod;

                if(personPeriod != null)
                {
                    personPeriod.StartDate = PeriodDateService.GetValidPeriodDate
                        (PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.PeoplePeriodView, person), date); 
                }
            }

            else if ((_contractBindingCollection.Count == 0)
                || (_partTimePercentageBindingCollection.Count == 0)
                || (_contractScheduleBindingCollection.Count == 0) ||
                filteredHolder.SiteTeamAdapterCollection.Count == 0)
                return null;
            else
            {
                DateOnly dateOnly = PeriodDateService.GetValidPeriodDate
                        (PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.PeoplePeriodView, person), date);
                personPeriod = new PersonPeriod(dateOnly,
                                                new PersonContract(_contractBindingCollection[0],
                                                                   _partTimePercentageBindingCollection[0],
                                                                   _contractScheduleBindingCollection[0]),
                                                filteredHolder.SiteTeamAdapterCollection[0].Team);
            }
            return personPeriod;
        }
        
        public static void RemovePersonSkillForSelectedPersonPeriods(IList<IPersonPeriodModel> selectedPersonPeriods,
            IPersonSkill personSkill)
        {
        	foreach (IPersonPeriodModel personPeriodModel in selectedPersonPeriods)
        	{
        		IPersonPeriod currentPeriod = personPeriodModel.Period;

        		if (currentPeriod != null)
        		{
        			personPeriodModel.Parent.RemoveSkill(personSkill.Skill, currentPeriod);
        			personPeriodModel.CanBold = true;
        		}
        	}
        }

        public void LoadContractSchedules(FilteredPeopleHolder filterPeopleHolder)
        {
            _contractScheduleBindingCollection.Clear();

            var repository = new ContractScheduleRepository(filterPeopleHolder.GetUnitOfWork);
            var list = repository.FindAllContractScheduleByDescription().Where(ptp => ptp.IsChoosable);

            foreach (IContractSchedule item in list)
            {
                _contractScheduleBindingCollection.Add(item);
            }
        }

        public void LoadPartTimePercentages(FilteredPeopleHolder filterPeopleHolder)
        {
            _partTimePercentageBindingCollection.Clear();
            var partTimePercentageRepository = new PartTimePercentageRepository(filterPeopleHolder.GetUnitOfWork);
            var list = partTimePercentageRepository.FindAllPartTimePercentageByDescription().Where(ptp => ptp.IsChoosable);

        	IEnumerable<IPartTimePercentage> sorted = list.OrderByDescending(n2 => n2.Percentage.Value);
        	list = sorted.ToList();

            foreach (IPartTimePercentage item in list)
            {
                _partTimePercentageBindingCollection.Add(item);
            }
        }

        public void LoadContracts(FilteredPeopleHolder filterPeopleHolder)
        {
            _contractBindingCollection.Clear();
            var repository = new ContractRepository(filterPeopleHolder.GetUnitOfWork);
            var list = repository.FindAllContractByDescription().Where(ptp => ptp.IsChoosable).ToList();

            foreach (IContract item in list)
            {
                _contractBindingCollection.Add(item);
            }
        }

    	public static void ChangePersonSkillActiveState(IList<IPersonPeriodModel> selectedPersonPeriods, IPersonSkill personSkill, bool active)
    	{
			if (selectedPersonPeriods == null) return;
    		foreach (var selectedPersonPeriod in selectedPersonPeriods)
    		{
				if (selectedPersonPeriod.Period == null) continue;
				var personSkillFromCollection = selectedPersonPeriod.Period.PersonSkillCollection.FirstOrDefault(s => s.Skill.Equals(personSkill.Skill));
    			if (personSkillFromCollection != null && personSkillFromCollection.Active != active)
    			{
					if (active)
					{
						selectedPersonPeriod.Parent.ActivateSkill(personSkill.Skill,selectedPersonPeriod.Period);
					}
					else
					{
						selectedPersonPeriod.Parent.DeactivateSkill(personSkill.Skill, selectedPersonPeriod.Period);
					}
					selectedPersonPeriod.CanBold = true;
    			}
    		}
    	}

		public static void SetPersonSkillForSelectedPersonPeriods(IList<IPersonPeriodModel> selectedPersonPeriods, IPersonSkill personSkill, Percent skillPercentage)
		{
			//TODO: This is happened when user is not selecting any row in grid and setting skills using skill panel
            // Need to come up with proper exception
            if (selectedPersonPeriods == null)
                return;

			foreach (IPersonPeriodModel personPeriodModel in selectedPersonPeriods)
			{
				IPersonPeriod currentPeriod = personPeriodModel.Period;

				if (currentPeriod != null)
				{
					IPersonSkill personSkillFromCollection = currentPeriod.PersonSkillCollection.FirstOrDefault(s => s.Skill.Equals(personSkill.Skill));
					if (personSkillFromCollection == null)
					{
						personPeriodModel.Parent.AddSkill(new PersonSkill(personSkill.Skill, skillPercentage), currentPeriod);
						personPeriodModel.CanBold = true;
					}
					else
					{
						if (personSkillFromCollection.SkillPercentage != skillPercentage)
						{
							personPeriodModel.Parent.ChangeSkillProficiency(personSkill.Skill,skillPercentage, currentPeriod);
							personPeriodModel.CanBold = true;
						}
					}
				}
			}
		}

    	public static void SetExternalLogOnForSelectedPersonPeriods(IList<IPersonPeriodModel> selectedPersonPeriods,
            IExternalLogOn externalLogOn)
        {
        	//TODO: This is happened when user is not selecting any row in grid and setting skills using skill panel
            // Need to come up with proper exception
            if (selectedPersonPeriods == null)
                return;

        	foreach (IPersonPeriodModel personPeriodModel in selectedPersonPeriods)
        	{
        		IPersonPeriod currentPeriod = personPeriodModel.Period;
        		var person = personPeriodModel.Parent;
        		if (currentPeriod != null)
        		{
        			IExternalLogOn externalLogOnFromCollection = currentPeriod.ExternalLogOnCollection.FirstOrDefault(s => s.Equals(externalLogOn));
        			if (externalLogOnFromCollection== null)
        			{
						person.AddExternalLogOn(externalLogOn, currentPeriod);
        				personPeriodModel.CanBold = true;
        			}
        		}
        	}
        }

        public static void RemoveExternalLogOnForSelectedPersonPeriods(IList<IPersonPeriodModel> selectedPersonPeriods,
           IExternalLogOn externalLogOn)
        {
        	foreach (IPersonPeriodModel personPeriodModel in selectedPersonPeriods)
        	{
        		IPersonPeriod currentPeriod = personPeriodModel.Period;
				var person = personPeriodModel.Parent;
        		if (currentPeriod != null)
        		{
        			IExternalLogOn externalLogOnFromCollection = currentPeriod.ExternalLogOnCollection.FirstOrDefault(s => s.Equals(externalLogOn));
        			if (externalLogOnFromCollection!=null)
        			{
						person.RemoveExternalLogOn(externalLogOnFromCollection, currentPeriod);
        				personPeriodModel.CanBold = true;
        			}
        		}
        	}
        }

    	public void LoadShiftCategories(IUnitOfWork unitOfWork)
        {
            IShiftCategoryRepository repository = new ShiftCategoryRepository(unitOfWork);
            _shiftCategories = repository.FindAll();
        }

        public void GetChildSchedulePeriods(int index, Collection<SchedulePeriodModel> schedulePeriodGridViewCollection,CommonNameDescriptionSetting commonNameDescription)
        {
            IList<ISchedulePeriod> schedulePeriodCollection = schedulePeriodGridViewCollection[index].Parent.PersonSchedulePeriodCollection;

            bool canBold = schedulePeriodGridViewCollection[index].CanBold;
            ISchedulePeriod currentSchedulePeriod = schedulePeriodGridViewCollection[index].SchedulePeriod;

            _schedulePeriodGridViewChildCollection = new List<SchedulePeriodChildModel>();
            CurrentChildName = commonNameDescription.BuildCommonNameDescription(schedulePeriodGridViewCollection[index].Parent);

            foreach (ISchedulePeriod schedulePeriod in schedulePeriodCollection)
            {
            	SchedulePeriodChildModel model =
            		EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(schedulePeriod);

            	if (((schedulePeriod == currentSchedulePeriod) && canBold) || schedulePeriod.Id == null)
            	{
            		model.CanBold = true;
            	}

            	model.FullName =commonNameDescription.BuildCommonNameDescription( schedulePeriodGridViewCollection[index].Parent);
            	_schedulePeriodGridViewChildCollection.Add(model);
            }
        }

        public void GetChildSchedulePeriods(int index, Collection<SchedulePeriodModel> schedulePeriodGridViewCollection,
            ReadOnlyCollection<SchedulePeriodChildModel> cachedSchedulePeriodChildCollection,CommonNameDescriptionSetting commonNameDescription)
        {
            IList<ISchedulePeriod> schedulePeriodCollection = schedulePeriodGridViewCollection[index].Parent.PersonSchedulePeriodCollection;

            bool canBold = schedulePeriodGridViewCollection[index].CanBold;
            ISchedulePeriod currentSchedulePeriod = schedulePeriodGridViewCollection[index].SchedulePeriod;

            _schedulePeriodGridViewChildCollection = new List<SchedulePeriodChildModel>();
            CurrentChildName =commonNameDescription.BuildCommonNameDescription( schedulePeriodGridViewCollection[index].Parent);

            foreach (ISchedulePeriod schedulePeriod in schedulePeriodCollection)
            {
            	SchedulePeriodChildModel model =
            		EntityConverter.ConvertToOther<ISchedulePeriod, SchedulePeriodChildModel>(schedulePeriod);

            	if (schedulePeriod != null && schedulePeriod.Id == null)
            	{
            		model.CanBold = true;
            	}
            	else
            	{
            		model.CanBold = PeopleAdminHelper.IsCanBold(schedulePeriod, cachedSchedulePeriodChildCollection);
            	}

            	if (schedulePeriod == currentSchedulePeriod)
            	{
            		// This is fixed for following secnario :If user click save button and user is changing current 
            		// period in the grid then he expands the grid changes still should be bold.
            		// (Apply only when model.Canbold is false)

            		if (!model.CanBold)
            		{
            			model.CanBold = canBold;
            		}
            	}

            	model.FullName = schedulePeriodGridViewCollection[index].Parent.Name.ToString();

            	_schedulePeriodGridViewChildCollection.Add(model);
            }
        }

        public ReadOnlyCollection<SchedulePeriodChildModel> SchedulePeriodChildGridData
        {
            get
            {
                _schedulePeriodGridViewChildCollection = _schedulePeriodGridViewChildCollection.OrderByDescending(n2 => n2.PeriodDate).ToList();

                return new ReadOnlyCollection<SchedulePeriodChildModel>(_schedulePeriodGridViewChildCollection);
            }
        }

        public ReadOnlyCollection<SchedulePeriodChildModel> SchedulePeriodChildGridDataWhenAddChild
        {
            get
            {
                SchedulePeriodChildModel model =
                    _schedulePeriodGridViewChildCollection[_schedulePeriodGridViewChildCollection.Count - 1];

                _schedulePeriodGridViewChildCollection.Remove(model);

                _schedulePeriodGridViewChildCollection = _schedulePeriodGridViewChildCollection.OrderByDescending(n2 => n2.PeriodDate).ToList();

                _schedulePeriodGridViewChildCollection.Insert(0, model);

                return new ReadOnlyCollection<SchedulePeriodChildModel>(_schedulePeriodGridViewChildCollection);
            }
        }

        public string CurrentRotationChildName
        {
            get;
            set;
        }

        public ReadOnlyCollection<PersonRotationModelChild> PersonRotationChildGridData
        {
            get { return new ReadOnlyCollection<PersonRotationModelChild>(_personRotationChildAdapterCollection); }
            set { _personRotationChildAdapterCollection = value; }
        }

        public ReadOnlyCollection<PersonAvailabilityModelChild> PersonAvailabilityChildGridData
        {
            get { return new ReadOnlyCollection<PersonAvailabilityModelChild>(_personAvailablityChildAdapterCollection); }
            set { _personAvailablityChildAdapterCollection = value; }
        }

        public ReadOnlyCollection<IPersonRotation> ChildPersonRotationCollection
        {
            get { return new ReadOnlyCollection<IPersonRotation>(_childrenPersonRotationCollection); }
            set { _childrenPersonRotationCollection = value; }
        }

        public ReadOnlyCollection<IPersonAvailability> ChildPersonAvailabilityCollection
        {
            get { return new ReadOnlyCollection<IPersonAvailability>(_childrenPersonAvailabilityCollection); }
            set { _childrenPersonAvailabilityCollection = value; }
        }

        public TypedBindingCollection<IRotation> AllRotations
        {

            get { return _rotationCollection; }
        }

        public TypedBindingCollection<IAvailabilityRotation> AllAvailabilities
        {

            get { return _availabilityCollection; }
        }

        public static void AddPersonRotation(IPersonRotation personRotation, int parentRowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {
            if (personRotation != null)
            {
                IPerson person = filteredPeopleHolder.PersonRotationParentAdapterCollection[parentRowIndex].Person;
                int index = filteredPeopleHolder.ParentPersonRotationCollection.
                    IndexOf(filteredPeopleHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Equals(person)));


                filteredPeopleHolder.AddNewPersonRotation(personRotation);

                if (filteredPeopleHolder.ParentPersonRotationCollection[index] == null)
                {
                    filteredPeopleHolder.ParentPersonRotationCollection[index] = personRotation;
                }
                filteredPeopleHolder.AllPersonRotationCollection.Add(personRotation);
            }
        }

        public static void AddPersonAvailability(IPersonAvailability personAvailability, int parentRowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {

            if (personAvailability != null)
            {
                filteredPeopleHolder.AddNewPersonAvailability(personAvailability);

                if (filteredPeopleHolder.ParentPersonAvailabilityCollection[parentRowIndex] == null)
                {
                    filteredPeopleHolder.ParentPersonAvailabilityCollection[parentRowIndex] = personAvailability;
                }
                filteredPeopleHolder.AllPersonAvailabilityCollection.Add(personAvailability);

            }
        }

        public void GetChildPersonRotations(int rowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPersonRotation selectedItem = filteredPeopleHolder.ParentPersonRotationCollection[rowIndex];

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                CurrentRotationChildName = filteredPeopleHolder.ParentPersonRotationCollection[rowIndex].Person.Name.ToString();

                _childrenPersonRotationCollection =
                    filteredPeopleHolder.AllPersonRotationCollection.Where(a => a.Person.Equals( selectedItem.Person)).ToList();

                IOrderedEnumerable<IPersonRotation> sorted = _childrenPersonRotationCollection.OrderByDescending(n2 => n2.StartDate);
                _childrenPersonRotationCollection = sorted.ToList();

                _personRotationChildAdapterCollection = new List<PersonRotationModelChild>();

                bool isFirstItem = true;
                foreach (IPersonRotation pRotation in _childrenPersonRotationCollection)
                {
                    var personRotationModel =
                        new PersonRotationModelChild(selectedItem.Person,filteredPeopleHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personRotationModel.PersonFullName = pRotation.Person.Name.ToString();
                        isFirstItem = false;
                    }
                    else
                        personRotationModel.PersonFullName = string.Empty;

                    personRotationModel.PersonRotation = pRotation;
                    
                    //if the rotation has not been set, use the default rotation
                    personRotationModel.CurrentRotation = pRotation.Rotation ?? getDefaultRotation();

                    _personRotationChildAdapterCollection.Add(personRotationModel);
                }
            }
            else
            {
                _childrenPersonRotationCollection.Clear();
            }
        }

        public void GetChildPersonAvailabilities(int rowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPersonAvailability selectedItem = filteredPeopleHolder.ParentPersonAvailabilityCollection[rowIndex];

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                CurrentRotationChildName = filteredPeopleHolder.ParentPersonAvailabilityCollection[rowIndex].Person.Name.ToString();

                _childrenPersonAvailabilityCollection =
                    filteredPeopleHolder.AllPersonAvailabilityCollection.Where(a => a.Person.Equals(selectedItem.Person)).ToList();

                IOrderedEnumerable<IPersonAvailability> sorted = _childrenPersonAvailabilityCollection.OrderByDescending(n2 => n2.StartDate);
                _childrenPersonAvailabilityCollection = sorted.ToList();

                _personAvailablityChildAdapterCollection = new List<PersonAvailabilityModelChild>();

                bool isFirstItem = true;
                foreach (IPersonAvailability availability in _childrenPersonAvailabilityCollection)
                {
                    var personAvailabilityAdapter =
                        new PersonAvailabilityModelChild(selectedItem.Person, availability,filteredPeopleHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personAvailabilityAdapter.PersonFullName = availability.Person.Name.ToString();
                        isFirstItem = false;
                    }
                    else
                        personAvailabilityAdapter.PersonFullName = string.Empty;


                    //if the rotation has not been set, use the default rotation
                    personAvailabilityAdapter.CurrentRotation = availability.Availability ?? getDefaultAvailability();

                    _personAvailablityChildAdapterCollection.Add(personAvailabilityAdapter);
                }
            }
            else
            {
                _childrenPersonAvailabilityCollection.Clear();
            }
        }

        public void LoadAllRotations(IUnitOfWork uow)
        {
            _rotationCollection.Clear();

            var rep = new RotationRepository(uow);
            var rotations = rep.LoadAllRotationsWithDays();

        	IEnumerable<IRotation> sorted = rotations.OrderBy(n2 => n2.Name);
        	rotations = sorted.ToList();
            rotations.ForEach(_rotationCollection.Add);
        }

        public void LoadAllAvailabilities(IUnitOfWork uow)
        {
            _availabilityCollection.Clear();
            var availabilities = new AvailabilityRepository(uow).LoadAllSortedByNameAscending();
            availabilities.ForEach(_availabilityCollection.Add);
        }

        public void LoadAllWorkflowControlSets(IUnitOfWork uow)
        {
            _workflowControlSetCollection.Clear();
            _workflowControlSetCollection.Add(PersonGeneralModel.NullWorkflowControlSet);

            var rep = new WorkflowControlSetRepository(uow);
            var workflowControlSets = rep.LoadAll();

        	IEnumerable<IWorkflowControlSet> sorted = workflowControlSets.OrderBy(n2 => n2.Name);
        	workflowControlSets = sorted.ToList();

            foreach (var workflowControlSet in workflowControlSets)
            {
                _workflowControlSetCollection.Add(workflowControlSet);
            }
        }

        public void GetParentPersonRotationWhenDeleted(int rowIndex, FilteredPeopleHolder filteredPeopleHolder)
        {
            IPersonRotation selectedPersonRotation = filteredPeopleHolder.ParentPersonRotationCollection[rowIndex];

            PersonRotationModelParent personRotationModelParent;

            GetChildPersonRotations(rowIndex, filteredPeopleHolder);

            IPersonRotation currentPersonRotation = filteredPeopleHolder.GetCurrentPersonRotation(_childrenPersonRotationCollection);

            if (currentPersonRotation != null)
            {
                filteredPeopleHolder.ParentPersonRotationCollection[rowIndex] = currentPersonRotation;

                personRotationModelParent = new PersonRotationModelParent(currentPersonRotation.Person, null)
                                            	{
                                            		PersonRotation = currentPersonRotation,
                                            		CurrentRotation = currentPersonRotation.Rotation,
                                            		RotationCount =
                                            			_childrenPersonRotationCollection.Count > 1
                                            				? _childrenPersonRotationCollection.Count
                                            				: 0
                                            	};

            	filteredPeopleHolder.ParentPersonRotationCollection[rowIndex] = currentPersonRotation;

            }
            else
            {
                personRotationModelParent = new PersonRotationModelParent(selectedPersonRotation.Person, null)
                                            	{
                                            		PersonRotation = currentPersonRotation,
                                            		RotationCount = _childrenPersonRotationCollection.Count > 0 ? 2 : 0
                                            	};

            	filteredPeopleHolder.ParentPersonRotationCollection[rowIndex] = GetSamplePersonRotation(selectedPersonRotation.Person);
            }

            filteredPeopleHolder.PersonRotationParentAdapterCollection[rowIndex] = personRotationModelParent;
        }

        public IPersonRotation GetSamplePersonRotation(IPerson person)
        {
            IRotation rotation = getDefaultRotation();

            var date = new DateOnly(PeopleAdminHelper.GetFirstDayOfWeek(DateTime.Today));

            if (rotation != null) return new PersonRotation(person, rotation, date, 0);
            
            return null;
        }

        public PersonAvailability GetSamplePersonAvailability(IPerson person)
        {
            IAvailabilityRotation availability = getDefaultAvailability();

            var date = new DateOnly(PeopleAdminHelper.GetFirstDayOfWeek(DateTime.Today));

            if (availability != null)
                return new PersonAvailability(person, availability, date);
            
            return null;
        }

        private IRotation getDefaultRotation()
        {
            if (_rotationCollection != null && _rotationCollection.Count > 0)
            {
                return _rotationCollection[0];
            }
            return null;
        }

        private IAvailabilityRotation getDefaultAvailability()
        {
            if (_availabilityCollection != null && _availabilityCollection.Count > 0)
            {
                return _availabilityCollection[0];
            }
            return null;
        }

        

        public ReadOnlyCollection<IPersonAccountChildModel> PersonAccountChildGridData
        {
            get
            {
                IOrderedEnumerable<IPersonAccountChildModel> sorted =
                    _personaccountGridViewChildCollection.OrderByDescending(n2 => n2.AccountDate);
                _personaccountGridViewChildCollection = sorted.ToList();

                return new ReadOnlyCollection<IPersonAccountChildModel>(_personaccountGridViewChildCollection);
            }
        }


        public ReadOnlyCollection<IPersonAccountChildModel> PersonAccountChildGridDataWhenAndChild
        {
            get
            {
                _personaccountGridViewChildCollection.Reverse();
                return new ReadOnlyCollection<IPersonAccountChildModel>(
                    _personaccountGridViewChildCollection);
            }
        }

        public void GetChildPersonAccounts(int index, 
								FilteredPeopleHolder filteredPeopleHolder)
        {


			Collection<IPersonAccountModel> personAccountGridViewAdaptorCollection =
				filteredPeopleHolder.PersonAccountModelCollection;
            IPerson personFiltered = personAccountGridViewAdaptorCollection[index].Parent.Person;

			IDictionary<IPerson, IPersonAccountCollection> allAccounts = filteredPeopleHolder.AllAccounts;
            var personAcccountCollection = allAccounts[personFiltered];

            bool canBold = personAccountGridViewAdaptorCollection[index].CanBold;
            IAccount currentAccount = personAccountGridViewAdaptorCollection[index].CurrentAccount;

            _personaccountGridViewChildCollection = new List<IPersonAccountChildModel>();
			CommonNameDescriptionSetting commonNameDescription = filteredPeopleHolder.CommonNameDescription;
            CurrentChildName = commonNameDescription.BuildCommonNameDescription(personFiltered);

	        foreach (var account in personAcccountCollection.AllPersonAccounts())
	        {
		        var adapter = new PersonAccountChildModel(filteredPeopleHolder.RefreshService, allAccounts[personFiltered], account,
		                                                  commonNameDescription,
		                                                  new FilteredPeopleAccountUpdater(filteredPeopleHolder, UnitOfWorkFactory.Current));
		        if (adapter.ContainedEntity != null && ((account == currentAccount) && canBold) ||
		            adapter.ContainedEntity.Id == null)
			        adapter.CanBold = true;
		        if (adapter.ContainedEntity != null)
		        {
			        using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			        {
				        filteredPeopleHolder.RefreshService.RefreshIfNeeded(adapter.ContainedEntity);
			        }
		        }
		        _personaccountGridViewChildCollection.Add(adapter);
	        }
        }

        public void GetChildPersonAccounts(int index, ReadOnlyCollection<IPersonAccountChildModel> cachedCollection, FilteredPeopleHolder filteredPeopleHolder)
        {

			Collection<IPersonAccountModel> personAccountGridViewAdaptorCollection =
				filteredPeopleHolder.PersonAccountModelCollection;
            IPerson personFiltered = personAccountGridViewAdaptorCollection[index].Parent.Person;

			IDictionary<IPerson, IPersonAccountCollection> allAccounts = filteredPeopleHolder.AllAccounts;
            var accountCollection = allAccounts[personFiltered];

            bool canBold = personAccountGridViewAdaptorCollection[index].CanBold;
            IAccount currentAccount = personAccountGridViewAdaptorCollection[index].CurrentAccount;

            _personaccountGridViewChildCollection = new List<IPersonAccountChildModel>();

			CommonNameDescriptionSetting commonNameDescription = filteredPeopleHolder.CommonNameDescription;
            CurrentChildName =commonNameDescription.BuildCommonNameDescription( personFiltered);

            foreach (var account in accountCollection.AllPersonAccounts())
            {
                var adapter = new PersonAccountChildModel(filteredPeopleHolder.RefreshService, allAccounts[personFiltered], account, commonNameDescription, new FilteredPeopleAccountUpdater(filteredPeopleHolder,UnitOfWorkFactory.Current) );
                handleCanBold(cachedCollection, canBold, currentAccount, account, adapter);
                _personaccountGridViewChildCollection.Add(adapter);   
            }
        }


        private static void handleCanBold(ReadOnlyCollection<IPersonAccountChildModel> cachedCollection,
            bool canBold, IAccount currentAccount, IAccount pa,
            IPersonAccountChildModel adapter)
        {
            if (adapter.ContainedEntity != null && adapter.ContainedEntity.Id == null)
            {
                adapter.CanBold = true;
            }
            else
            {
                adapter.CanBold = PeopleAdminHelper.IsCanBold(pa, cachedCollection);
            }

            if (pa == currentAccount)
            {

                // This is fixed for following secnario :If user click save button and user is changing current 
                // period in the grid then he expands the grid changes still should be bold.
                // (Apply only when adapter.Canbold is false)
                if (!adapter.CanBold)
                {
                    adapter.CanBold = canBold;
                }
            }
        }

        public void LoadRotationStateHolder(ViewType type, FilteredPeopleHolder filteredPeopleHolder)
        {
            if (_rotationStateHolderCache.ContainsKey(type))
            {
                RotationStateHolder = _rotationStateHolderCache[type];
				RotationStateHolder.FilteredStateHolder = filteredPeopleHolder;
            }
            else
            {
                IRotationStateHolder rotationStateHolder = CreatorRotationStateHolder(type, filteredPeopleHolder);

                InParameter.NotNull("rotationStateHolder", rotationStateHolder);

                RotationStateHolder = rotationStateHolder;

                _rotationStateHolderCache.Add(type, rotationStateHolder);
            }
        }

        public IRotationStateHolder RotationStateHolder
        {
            get;
            set;
        }

        public IList<IShiftCategory> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public TypedBindingCollection<IWorkflowControlSet> WorkflowControlSetCollection
        {
            get {
                return _workflowControlSetCollection;
            }
        }


        public IRotationStateHolder CreatorRotationStateHolder(ViewType type, FilteredPeopleHolder filteredStateHolder)
        {
            switch (type)
            {
                case ViewType.PersonRotationView:
                    {
                        return new PersonRotationStateholder(filteredStateHolder, this);
                    }
                case ViewType.PersonAvailabilityView:
                    {
                        return new PersonAvailabilityStateholder(filteredStateHolder, this);
                    }
                default:
                    return null;
            }
        }
    }
}
