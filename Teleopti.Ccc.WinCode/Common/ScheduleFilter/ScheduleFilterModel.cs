using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.ScheduleFilter
{
    public class ScheduleFilterModel : IGroupPageDataProvider
    {
        private readonly ICollection<IPerson> _selectedPersons;
        private readonly ICollection<IPerson> _orgSelectedPersons = new List<IPerson>();
        private readonly IList<IPerson> _persons;
        private IBusinessUnit _businessUnit;
        private readonly DateOnlyPeriod _filterPeriod;
        private IList<IContract> _contractCollection;
        private IList<IContractSchedule> _contractScheduleCollection;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IList<ISkill> _skillCollection;

        private readonly IRepository<IContract> _contractRepository;
        private readonly IContractScheduleRepository _contractScheduleRepository;
        private readonly IRepository<IPartTimePercentage> _partTimePercentageRepository;
        private readonly IRepository<IRuleSetBag> _ruleSetBagRepository;
        private readonly IGroupPageRepository _groupPageRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBusinessUnitRepository _businessUnitRepository;
        private IList<IGroupPage> _groupPages;
        private readonly CommonNameDescriptionSetting _commonNameDescriptionSetting;
        private DateTime _filterDateUtc = DateTime.MinValue;
        private readonly IDictionary<TeamPersonKey, IPerson> _selectedPersonDictionary = new Dictionary<TeamPersonKey, IPerson>();

        public ScheduleFilterModel(ICollection<IPerson> selectedPersons, ISchedulerStateHolder stateHolder,
            IRepository<IContract> contractRepository,
            IContractScheduleRepository contractScheduleRepository,
            IRepository<IPartTimePercentage> partTimePercentageRepository,
            IRepository<IRuleSetBag> ruleSetBagRepository,
            IGroupPageRepository groupPageRepository,
            ISkillRepository skillRepository,
            IBusinessUnitRepository businessUnitRepository,
            DateTime defaultFilterDate
            )
        {
            _selectedPersons = selectedPersons;
            CopyPersonsToOrgList(selectedPersons);
            _persons = stateHolder.AllPermittedPersons;
            _filterPeriod = stateHolder.RequestedPeriod.DateOnlyPeriod;
            _commonNameDescriptionSetting = stateHolder.CommonNameDescription;
            _contractRepository = contractRepository;
            _contractScheduleRepository = contractScheduleRepository;
            _partTimePercentageRepository = partTimePercentageRepository;
            _ruleSetBagRepository = ruleSetBagRepository;
            _groupPageRepository = groupPageRepository;
            _skillRepository = skillRepository;
            _businessUnitRepository = businessUnitRepository;
            _filterDateUtc = defaultFilterDate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void CopyPersonsToOrgList(IEnumerable<IPerson> selectedPersons)
        {
            _orgSelectedPersons.Clear();
            foreach (IPerson person in selectedPersons)
            {
                _orgSelectedPersons.Add(person);
            }
        }

        public void CopyPersonsFromOrgList()
        {
            _selectedPersons.Clear();
            foreach (IPerson person in _orgSelectedPersons)
            {
                _selectedPersons.Add(person);
            }
        }

        public string CommonAgentName(IPerson person)
        {
            return _commonNameDescriptionSetting.BuildCommonNameDescription(person);
        }

        public IEnumerable<IPerson> PersonCollection
        {
            get { return _persons; }
        }
        public IBusinessUnit BusinessUnit
        {
            get
            {
                if (_businessUnit == null)
                    _businessUnit = _businessUnitRepository.Get(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault());

                return _businessUnit;
            }
        }

        public DateOnly FilterDate
        {
            get
            {
                ICccTimeZoneInfo cccTimeZoneInfo = TeleoptiPrincipal.Current.Regional.TimeZone;
                return new DateOnly(cccTimeZoneInfo.ConvertTimeFromUtc(FilterDateUtc, cccTimeZoneInfo));
            }
        }

        public static DateTimePeriod FilterPeriodUtc(DateTime dateTime)
        {
            ICccTimeZoneInfo cccTimeZoneInfo = TeleoptiPrincipal.Current.Regional.TimeZone;
            DateTime utcDateTime = cccTimeZoneInfo.ConvertTimeToUtc(dateTime, cccTimeZoneInfo);

            return new DateTimePeriod(utcDateTime, utcDateTime.AddDays(1).AddMilliseconds(-1));
        }
        public DateOnlyPeriod FilterPeriod
        {
            get { return _filterPeriod; }
        }

        public ICollection<IPerson> SelectedPersons
        {
            get { return _selectedPersons; }
        }

        public ICollection<IPerson> OrgSelectedPersons
        {
            get { return _orgSelectedPersons; }
        }

        public IEnumerable<IContract> ContractCollection
        {
            get
            {
                if (_contractCollection == null)
                    _contractCollection = _contractRepository.LoadAll();
                return _contractCollection;
            }
        }

        public IEnumerable<IContractSchedule> ContractScheduleCollection
        {
            get
            {
                if (_contractScheduleCollection == null)
                    _contractScheduleCollection = new List<IContractSchedule>(_contractScheduleRepository.LoadAllAggregate());
                return _contractScheduleCollection;
            }
        }

        public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
        {
            get
            {
                if (_partTimePercentageCollection == null)
                    _partTimePercentageCollection = _partTimePercentageRepository.LoadAll();
                return _partTimePercentageCollection;
            }
        }

        public IEnumerable<IRuleSetBag> RuleSetBagCollection
        {
            get
            {
                if (_ruleSetBagCollection == null)
                    _ruleSetBagCollection = _ruleSetBagRepository.LoadAll();
                return _ruleSetBagCollection;
            }
        }

        public IEnumerable<IGroupPage> UserDefinedGroupings
        {
            get
            {
                return _groupPageRepository.LoadAllGroupPageBySortedByDescription();
            }
        }

        public IEnumerable<IBusinessUnit> BusinessUnitCollection
        {
            get { yield return _businessUnit; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get
            {
                return _filterPeriod;
            }
        }

        public IList<ISkill> SkillCollection
        {
            get
            {
                if (_skillCollection == null)
                    _skillCollection = _skillRepository.LoadAll();
                return _skillCollection;
            }
        }

        public  IList<IGroupPage> GroupPages
        {
            get
            {
                if (_groupPages == null)
                {
                    GroupingsCreator groupingsCreator = new GroupingsCreator(this);
                    _groupPages = groupingsCreator.CreateBuiltInGroupPages(false);
                    foreach (IGroupPage userDefinedGrouping in UserDefinedGroupings)
                    {
                        _groupPages.Add(userDefinedGrouping);
                    }
                }
                return _groupPages;
            }
        }

        public DateTime FilterDateUtc
        {
            get { return _filterDateUtc;}
            set { _filterDateUtc = value; }
        }

        public IDictionary<TeamPersonKey, IPerson> SelectedPersonDictionary
        {
            get { return _selectedPersonDictionary; }
        }

        public void TransferSelectedPersonDictionaryToSelectedPersons()
        {
            var uniquePersons = new HashSet<IPerson>(_selectedPersonDictionary.Values);

            _selectedPersons.Clear();
            foreach (var uniquePerson in uniquePersons)
            {
                _selectedPersons.Add(uniquePerson);
            }
        }
    }

    public class TeamPersonKey
    {
        private readonly ITeam _team;
        private readonly IPerson _person;

        public TeamPersonKey(ITeam team, IPerson person)
        {
            _team = team;
            _person = person;
        }

        public ITeam Team
        {
            get { return _team; }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public override int GetHashCode()
        {
            return Team.GetHashCode() ^ Person.GetHashCode();
        }
    }
}
