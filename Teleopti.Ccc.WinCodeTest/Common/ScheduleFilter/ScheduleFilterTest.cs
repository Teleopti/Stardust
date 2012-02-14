using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.ScheduleFilter;
using Teleopti.Interfaces.Domain;
using System.Drawing;


namespace Teleopti.Ccc.WinCodeTest.Common.ScheduleFilter
{
    [TestFixture]
    public class ScheduleFilterTest
    {
        private MockRepository _mocks;
        private IScheduleFilterView _view;
        private ScheduleFilterModel _model;
        private ScheduleFilterPresenter _target;
        private ISchedulerStateHolder _stateHolder;
        private IList<IPerson> _persons = new List<IPerson>();
        private IList<IPerson> _selectedPersons = new List<IPerson>();
        private DateTimePeriod _period = new DateTimePeriod(2000, 1, 1, 2001, 12, 31);
        private ISite _site;
        private ISite _site2;
        private ITeam _team1;
        private ITeam _team2;
        private IPerson _person;
        private IPerson _person2;
        private IPerson _person3;
        private IPerson _person4;
        private IRepository<IContract> _contractRepository;
        private IContractScheduleRepository _contractScheduleRepository;
        private IRepository<IPartTimePercentage> _partTimePercentageRepository;
        private IRepository<IRuleSetBag> _ruleSetBagRepository;
        private IGroupPageRepository _groupPageRepository;
        private ISkillRepository _skillRepository;
        private IList<ISkill> _skills = new List<ISkill>();
        private IList<IContract> _contracts;
        private ICollection<IContractSchedule> _contractSchedules = new List<IContractSchedule>();
        private IList<IPartTimePercentage> _partTimePercentage = new List<IPartTimePercentage>();
        private IList<IRuleSetBag> _ruleSetBags = new List<IRuleSetBag>();
        private IGroupPage _groupPage = new GroupPage("A page");
        private IList<IGroupPage> _groupPages;
        private IContractSchedule _contractSchedule;
        private DateTime _filterDateTime =  new DateTime(2009, 11, 3, 23, 0, 0, DateTimeKind.Utc);
        private DateTime _dateTimeFromDatePicker;
        private IBusinessUnitRepository _businessUnitRepository;
        private IBusinessUnit _businessUnit;
        private readonly Guid _businessUnitId = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();

        [SetUp]
        public void Setup()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            _contractSchedule = ContractScheduleFactory.CreateContractSchedule("ContractSchedule");
            _contractSchedules.Add(_contractSchedule);
            _groupPages = new List<IGroupPage>{_groupPage};
            IContract contractFixed = ContractFactory.CreateContract("Fixed");
            IContract contractHourly = ContractFactory.CreateContract("Hourly");
            _contracts = new List<IContract>{contractHourly, contractFixed};

            _selectedPersons.Add(PersonFactory.CreatePerson("selected person"));
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IScheduleFilterView>();
            
            createRepositories();
            instantiatePersons();
            
            _stateHolder = new SchedulerStateHolder(scenario, _period, _persons);
            _businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
            _model = new ScheduleFilterModel(_selectedPersons, _stateHolder,
                _contractRepository, _contractScheduleRepository, _partTimePercentageRepository, 
                _ruleSetBagRepository, _groupPageRepository, _skillRepository, _businessUnitRepository, _filterDateTime);
            _target = new ScheduleFilterPresenter(_view, _model);

            loadPersons();
        }

        private void createRepositories()
        {
            _contractRepository = _mocks.StrictMock<IRepository<IContract>>();
            _contractScheduleRepository = _mocks.StrictMock<IContractScheduleRepository>();
            _partTimePercentageRepository = _mocks.StrictMock<IRepository<IPartTimePercentage>>();
            _groupPageRepository = _mocks.StrictMock<IGroupPageRepository>();
            _ruleSetBagRepository = _mocks.StrictMock<IRepository<IRuleSetBag>>();
            _skillRepository = _mocks.StrictMock<ISkillRepository>();
            _businessUnitRepository = _mocks.StrictMock<IBusinessUnitRepository>();
        }

        [Test]
        public void CanGetBusinessUnit()
        {
            Assert.IsNotNull(_target.BusinessUnit);
        }

        [Test]
        public void CanGetCurrentFilterDate()
        {
            Assert.AreEqual(_filterDateTime, _target.CurrentFilterDate);
        }

        [Test]
        public void CanInitializePresenter()
        {
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            using (_mocks.Record())
            {
                _view.ButtonOkText(Resources.Ok);
                _view.ButtonCancelText(Resources.Cancel);
                _view.SetColor();
                _view.SetTexts();
                Expect.Call(_contractRepository.LoadAll()).Return(_contracts);
                Expect.Call(_contractScheduleRepository.LoadAllAggregate()).Return(_contractSchedules);
                Expect.Call(_groupPageRepository.LoadAllGroupPageBySortedByDescription()).Return(_groupPages);
                Expect.Call(_partTimePercentageRepository.LoadAll()).Return(_partTimePercentage);
                Expect.Call(_ruleSetBagRepository.LoadAll()).Return(_ruleSetBags);
                Expect.Call(() => _view.CreateAndAddTreeNode(cccTreeNode)).Repeat.Once().IgnoreArguments();
                _view.AddTabPages(Resources.BusinessUnitHierarchy);
                Expect.Call(() => _view.AddTabPages(_groupPage)).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_skillRepository.LoadAll()).Return(_skills);
                Expect.Call(_businessUnitRepository.Get(_businessUnitId)).Return(_businessUnit);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
            Assert.NotNull(_model.FilterDate);
        }

        [Test]
        public void CanInitializeTabPages()
        {
            using (_mocks.Record())
            {
                Expect.Call(_contractRepository.LoadAll()).Return(_contracts);
                Expect.Call(_groupPageRepository.LoadAllGroupPageBySortedByDescription()).Return(new List<IGroupPage>());
                Expect.Call(_contractScheduleRepository.LoadAllAggregate()).Return(_contractSchedules);
                Expect.Call(_partTimePercentageRepository.LoadAll()).Return(_partTimePercentage);
                Expect.Call(_ruleSetBagRepository.LoadAll()).Return(_ruleSetBags);
                Expect.Call(() => _view.AddTabPages(_groupPage)).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_skillRepository.LoadAll()).Return(_skills);
            }
            using (_mocks.Playback())
            {
                _target.InitializeTabPages();
            }
        }
        
        [Test]
        public void VerifyCheckRootWhenNoPersonsAvailable()
        {
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            _persons.Clear();
            _model = new ScheduleFilterModel(_selectedPersons, _stateHolder,
                _contractRepository, _contractScheduleRepository, _partTimePercentageRepository, 
                _ruleSetBagRepository, _groupPageRepository, _skillRepository, _businessUnitRepository, _filterDateTime);
            _target = new ScheduleFilterPresenter(_view, _model);

            using (_mocks.Record())
            {
                Expect.Call(()=>_view.CreateAndAddTreeNode(cccTreeNode)).Repeat.AtLeastOnce().IgnoreArguments();
                Expect.Call(_businessUnitRepository.Get(_businessUnitId)).Return(_businessUnit);
            }
            using (_mocks.Playback())
            {
                _target.InitializeMainView();
            }
        }

        [Test]
        public void CanInitializeTreeView()
        {
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            using (_mocks.Record())
            {
                Expect.Call(()=>_view.CreateAndAddTreeNode(cccTreeNode)).Repeat.AtLeastOnce().IgnoreArguments();
                Expect.Call(_businessUnitRepository.Get(_businessUnitId)).Return(_businessUnit);
            }
            using (_mocks.Playback())
            {
                _target.InitializeMainView();
            }
        }

        [Test]
        public void CanInitializeTabPagesAndAddUnderGroups()
        {
            IGroupPage groupPage = new GroupPage("A group page");
            IRootPersonGroup rootPersonGroup = new RootPersonGroup("Bandybarn");
            IChildPersonGroup rootPersonChildGroup = new ChildPersonGroup("Bandybarnbarn");
            rootPersonGroup.AddPerson(_persons[0]);
            rootPersonChildGroup.AddPerson(_persons[1]);
            rootPersonGroup.AddChildGroup(rootPersonChildGroup);
            groupPage.AddRootPersonGroup(rootPersonGroup);
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            using (_mocks.Record())
            {
                Expect.Call(() => _view.CreateAndAddTreeNode(cccTreeNode)).Repeat.Once().IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.AddGroupingTreeStructure(groupPage);
            }
        }

        [Test]
        public void CanCloseForm()
        {
            using (_mocks.Record())
            {
                _view.CloseFilterForm();
            }
            using (_mocks.Playback())
            {
                _target.OnCloseForm();
            }
        }
        [Test]
        public void CanCancel()
        {
            using (_mocks.Record())
            {
                _view.CloseFilterForm();
            }
            using (_mocks.Playback())
            {
                _target.OnCancel();
            }
        }
        
        [Test]
        public void ShouldOpenContextMenu()
        {
            using (_mocks.Record())
            {
                _view.OpenContextMenu(new Point());
            }
            using (_mocks.Playback())
            {
                _target.OnMouseClick(new Point());
            }
        }

        [Test]
        public void ShouldOpenSearchWindow()
        {
            using (_mocks.Record())
            {
                _view.DisplaySearch();
            }
            using (_mocks.Playback())
            {
                _target.OnToolStripMenuItemSearch();
            }
        }

        [Test]
        public void ShouldReturnPersonList()
        {
            Assert.AreEqual(_model.PersonCollection, _target.PersonCollection);
        }


        [Test]
        public void OrgSelectedPersonListIsResetOnCancel()
        {
            int intitialNumberOfSelectedPersons = _model.SelectedPersons.Count;
            Assert.IsTrue(_model.OrgSelectedPersons.Count == _model.SelectedPersons.Count);
            _model.SelectedPersons.Add(_person2);

            Assert.IsTrue(_model.SelectedPersons.Count == intitialNumberOfSelectedPersons + 1);
            Assert.IsTrue(_model.OrgSelectedPersons.Count != _model.SelectedPersons.Count);

            _target.OnCancel();

            Assert.IsTrue(_model.SelectedPersons.Count == intitialNumberOfSelectedPersons);
            Assert.IsFalse(_model.SelectedPersons.Contains(_person2));
        }

        [Test]
        public void CanHandleLoadingOfTreesWhenSelectedTabIsChangedToGroupingTab()
        {
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            using (_mocks.Record())
            {
                _view.ClearSelectedTabControls();
                Expect.Call(()=> _view.CreateAndAddTreeNode(cccTreeNode)).Repeat.AtLeastOnce().IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.OnTabPageSelectedIndexChanged(_groupPage);
            }
        }
        [Test]
        public void CanHandleLoadingOfTreesWhenSelectedTabIsChangedToBusinessHierarchyTab()
        {
            CccTreeNode cccTreeNode = new CccTreeNode("Contracts", _persons, false, 0);
            using (_mocks.Record())
            {
                _view.ClearSelectedTabControls();
                Expect.Call(() => _view.CreateAndAddTreeNode(cccTreeNode)).Repeat.AtLeastOnce().IgnoreArguments();
                Expect.Call(_businessUnitRepository.Get(_businessUnitId)).Return(_businessUnit);
            }
            using (_mocks.Playback())
            {
                _target.OnTabPageSelectedIndexChanged("string");
            }
        }

        [Test]
        public void CanDecideIfExpandedParents()
        {
            CccTreeNode grandParent = new CccTreeNode("Grand Parent", _persons, false, 0);
            CccTreeNode parent = new CccTreeNode("Parent", _persons, false, 0);
            CccTreeNode child = new CccTreeNode("Child", _persons, false, 0);
            child.Parent = parent;
            parent.Parent = grandParent;

            grandParent.Nodes.Add(parent);
            parent.Nodes.Add(child);
            ScheduleFilterPresenter.ExpandParents(child);

            Assert.IsFalse(grandParent.DisplayExpanded);
            Assert.IsFalse(parent.DisplayExpanded);

            child = new CccTreeNode("Child", _persons, true, 0);
            child.Parent = parent;
            parent.Nodes.Clear();
            parent.Nodes.Add(child);

            ScheduleFilterPresenter.ExpandParents(child);

            Assert.IsTrue(grandParent.DisplayExpanded);
            Assert.IsTrue(parent.DisplayExpanded);
        }

        [Test]
        public void CanReturnAOneDayUtcFilterPeriod()
        {
            _dateTimeFromDatePicker = new DateTime(2009, 11, 1, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime dateTime1 = new DateTime(2009, 10, 31, 23, 0, 0, DateTimeKind.Utc);
            DateTime dateTime2 = dateTime1.AddDays(1).AddMilliseconds(-1);
            DateTimePeriod expectedDateTimePeriod = new DateTimePeriod(dateTime1, dateTime2);
            Assert.AreEqual(expectedDateTimePeriod, ScheduleFilterModel.FilterPeriodUtc(_dateTimeFromDatePicker));
        }

        [Test]
        public void CanGetCommonAgentDescription()
        {
            Assert.IsNotNull(_model.CommonAgentName(_person));
        }

        private void loadPersons()
        {
            IContractScheduleWeek contractScheduleWeek = new ContractScheduleWeek();
            contractScheduleWeek.Add(DayOfWeek.Friday, true);
            contractScheduleWeek.Add(DayOfWeek.Saturday, true);
            _contractSchedule.AddContractScheduleWeek(contractScheduleWeek);

            _site = SiteFactory.CreateSiteWithOneTeam("Mammas pojkar");
            _site2 = SiteFactory.CreateSiteWithOneTeam("Pappas flickor");

            _businessUnit.AddSite(_site);
            _businessUnit.AddSite(_site2);

            _team1 = _site.TeamCollection[0];
            _team2 = _site2.TeamCollection[0];

            _person.PersonPeriodCollection[0].Team = _team1;
            _person.PersonPeriodCollection[0].PersonContract.ContractSchedule = _contractSchedule;
            _person2.PersonPeriodCollection[0].Team = _team1;
            _person2.PersonPeriodCollection[0].PersonContract.ContractSchedule = _contractSchedule;
            _person3.PersonPeriodCollection[0].Team = _team1;
            _person3.PersonPeriodCollection[0].PersonContract.ContractSchedule = _contractSchedule;
            _person4.PersonPeriodCollection[0].Team = _team2;
            _person4.PersonPeriodCollection[0].PersonContract.ContractSchedule = _contractSchedule;
        }

        private void instantiatePersons()
        {
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1), new List<ISkill>());
            _person.Name = new Name("PersonA", "PersonA");
            _persons.Add(_person);
            _person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1), new List<ISkill>());
            _person2.Name = new Name("PersonB", "PersonB");_persons.Add(_person2);
            _persons.Add(_person2);
            _person3 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1), new List<ISkill>());
            _person3.Name = new Name("PersonC", "PersonC");_persons.Add(_person3);
            _persons.Add(_person3);
            _person4 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1), new List<ISkill>());
            _person4.Name = new Name("PersonD", "PersonD");_persons.Add(_person4);
            _persons.Add(_person4);
        }
    }
}
