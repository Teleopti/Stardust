using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture, SetUICulture("en-US")]
    public class MultiplicatorDefinitionPresenterTest
    {
        private MockRepository _mockRepository;
        private IUnitOfWork _unitOfWork;
        private IPayrollHelper _payrollHelper;
        private IExplorerPresenter _explorerPresenter;
        private IExplorerView _explorereView;
        private IMultiplicatorDefinitionPresenter target;

        private DayOfWeekMultiplicatorDefinition _dayOfWeekMultiplicatorDefinition;
        private DateTimeMultiplicatorDefinition _dateTimeMultiplicatorDefinition;
       
        private IMultiplicatorDefinitionSet _definitionSet1;
        private IList<IMultiplicatorDefinitionSet> _definitionSetCollection;

        private IMultiplicator _multiplicator1;
        private IList<IMultiplicator> _multiplicatorCollection;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
            _payrollHelper = _mockRepository.StrictMock<IPayrollHelper>();
            _explorereView = _mockRepository.StrictMock<IExplorerView>();

            _multiplicator1 = new Multiplicator(MultiplicatorType.OBTime);
            _multiplicator1.Description=new Description("OB1", "O");
            _multiplicator1.MultiplicatorValue = 1.5d;
            
            _definitionSet1 = new MultiplicatorDefinitionSet("test1", MultiplicatorType.OBTime);
            
            _dayOfWeekMultiplicatorDefinition = new DayOfWeekMultiplicatorDefinition(_multiplicator1, DayOfWeek.Tuesday,
                                                                                      new TimePeriod(1, 1, 2, 2));
            _dateTimeMultiplicatorDefinition = new DateTimeMultiplicatorDefinition(_multiplicator1,
                                                                                    new DateOnly(2010, 1, 1),
                                                                                    new DateOnly(2010, 1, 1),
                                                                                    new TimeSpan(10, 0, 0),
                                                                                    new TimeSpan(12, 0, 0));

            _definitionSetCollection = new List<IMultiplicatorDefinitionSet>();
            _definitionSetCollection.Add(_definitionSet1);

            _multiplicatorCollection = new List<IMultiplicator>();
            _multiplicatorCollection.Add(_multiplicator1);
            
            using (_mockRepository.Record())
            {
                Expect.On(_explorereView).Call(_explorereView.UnitOfWork).Return(_unitOfWork).Repeat.Any();
                Expect.On(_payrollHelper).Call(_payrollHelper.UnitOfWork).Return(_unitOfWork).Repeat.Any();
                Expect.On(_payrollHelper).Call(_payrollHelper.LoadMultiplicatorList()).Return(_multiplicatorCollection).Repeat.Any();
                Expect.On(_payrollHelper).Call(_payrollHelper.LoadDefinitionSets()).Return(_definitionSetCollection).Repeat.Any();
            }
           
            _mockRepository.ReplayAll();
            _explorerPresenter = new ExplorerPresenter(_payrollHelper, _explorereView);
            _explorerPresenter.Model.MultiplicatorCollection = _multiplicatorCollection;

            target = new MultiplicatorDefinitionPresenter(_explorerPresenter);

        }

        [Test]
        public void VerifyAddNew()
        {
            target.AddNewDayOfWeek(_definitionSet1);
            Assert.AreEqual(1, target.ModelCollection.Count);
            target.AddNewDateTime(_definitionSet1);
            Assert.AreEqual(2, target.ModelCollection.Count);

            //Check type
            Assert.AreEqual(typeof(DayOfWeekMultiplicatorDefinition), target.ModelCollection[0].MultiplicatorDefinitionType.MultiplicatorDefinitionType);
            //Add a new date time to first (0) (change type of it)
            target.AddNewDateTimeAt(_definitionSet1,0,_multiplicator1);

            Assert.AreEqual(3, target.ModelCollection.Count);
            Assert.AreEqual(typeof(DateTimeMultiplicatorDefinition), target.ModelCollection[0].MultiplicatorDefinitionType.MultiplicatorDefinitionType);

            //Check type
            Assert.AreEqual(typeof(DateTimeMultiplicatorDefinition), target.ModelCollection[0].MultiplicatorDefinitionType.MultiplicatorDefinitionType);

            //Add a new dayOfWeek to first (0) (change type of it)
            target.AddNewDayOfWeekAt(_definitionSet1, 0, _multiplicator1);
            Assert.AreEqual(4, target.ModelCollection.Count);

            Assert.AreEqual(typeof(DayOfWeekMultiplicatorDefinition), target.ModelCollection[0].MultiplicatorDefinitionType.MultiplicatorDefinitionType);
        
        }

        [Test]
        public void VerifyAddNewAt()
        {
            IList<IMultiplicatorDefinitionAdapter> multiplicatorDefinitionTypeCollection = new List<IMultiplicatorDefinitionAdapter>();
            multiplicatorDefinitionTypeCollection.Add(
                new MultiplicatorDefinitionAdapter(typeof(DayOfWeekMultiplicatorDefinition), "Day Of Week"));
            multiplicatorDefinitionTypeCollection.Add(
                new MultiplicatorDefinitionAdapter(typeof(DateTimeMultiplicatorDefinition), "From - To"));

            // Test to add definition of type day of week
            IMultiplicatorDefinitionViewModel model = new MultiplicatorDefinitionViewModel(
                    new DayOfWeekMultiplicatorDefinition(_multiplicator1, DayOfWeek.Friday,
                                                         new TimePeriod("18-22")));
            int orderIndex = 0;
            target.AddNewDateTime(_definitionSet1);
            target.AddNewDayOfWeek(_definitionSet1);
            target.AddNewAt(_definitionSet1, model, orderIndex, multiplicatorDefinitionTypeCollection);

            IMultiplicatorDefinitionViewModel viewModel = target.ModelCollection[orderIndex];
            Assert.AreEqual(typeof(DayOfWeekMultiplicatorDefinition),
                            viewModel.MultiplicatorDefinitionType.MultiplicatorDefinitionType);
            Assert.AreEqual(DayOfWeek.Friday, viewModel.DayOfWeek);
            Assert.AreEqual(new TimeSpan(18, 0, 0), viewModel.StartTime);
            Assert.AreEqual(new TimeSpan(22, 0, 0), viewModel.EndTime);
            Assert.IsNull(viewModel.FromDate);
            Assert.IsNull(viewModel.ToDate);
            Assert.AreEqual(_multiplicator1.Description, viewModel.Multiplicator.Description);


            IMultiplicatorDefinitionViewModel model2 =
                new MultiplicatorDefinitionViewModel(
                    new DateTimeMultiplicatorDefinition(_multiplicator1, new DateOnly(2010,1,19),new DateOnly(2010,1,19),new TimeSpan(17,0,0),new TimeSpan(23,0,0)));
            
            orderIndex = 1;
            target.AddNewAt(_definitionSet1, model2, orderIndex, multiplicatorDefinitionTypeCollection);

            viewModel = target.ModelCollection[orderIndex];
            Assert.AreEqual(typeof(DateTimeMultiplicatorDefinition),
                            viewModel.MultiplicatorDefinitionType.MultiplicatorDefinitionType);
            Assert.AreEqual(null, viewModel.DayOfWeek);
            Assert.AreEqual(null, viewModel.StartTime);
            Assert.AreEqual(null, viewModel.EndTime);
            Assert.AreEqual(model2.FromDate, viewModel.FromDate);
            Assert.AreEqual(model2.ToDate, viewModel.ToDate);
            Assert.AreEqual(_multiplicator1.Description, viewModel.Multiplicator.Description);
        }

        [Test]
        public void VerifyMoveUp()
        {
            target.AddNewDayOfWeek(_definitionSet1);
            target.AddNewDayOfWeek(_definitionSet1);
            target.AddNewDayOfWeek(_definitionSet1);

            MultiplicatorDefinitionViewModel two = (MultiplicatorDefinitionViewModel) target.ModelCollection[1];
        
            Assert.AreEqual(1, two.DomainEntity.OrderIndex);
   
            target.MoveUp(_definitionSet1,two.DomainEntity);
            Assert.AreEqual(0, two.DomainEntity.OrderIndex);
        }

        [Test]
        public void VerifyMoveDown()
        {
            target.AddNewDayOfWeek(_definitionSet1);
            target.AddNewDayOfWeek(_definitionSet1);
            target.AddNewDayOfWeek(_definitionSet1);

            MultiplicatorDefinitionViewModel two = (MultiplicatorDefinitionViewModel)target.ModelCollection[1];

            Assert.AreEqual(1, two.DomainEntity.OrderIndex);

            target.MoveDown(_definitionSet1, two.DomainEntity);
            Assert.AreEqual(2, two.DomainEntity.OrderIndex);
        }

        [Test]
        public void VerifyDelete()
        {
            target.AddNewDateTime(_definitionSet1);
            target.AddNewDateTime(_definitionSet1);
            Assert.AreEqual(2, target.ModelCollection.Count);

            MultiplicatorDefinitionViewModel deleteMe = (MultiplicatorDefinitionViewModel)target.ModelCollection[1];
            
            target.DeleteSelected(_definitionSet1, deleteMe.DomainEntity);
            Assert.AreEqual(1, target.ModelCollection.Count);
            Assert.False(target.ModelCollection.Contains(deleteMe));

            target.DeleteSelected(_definitionSet1, null);

        }

        [Test]
        public void VerifySort()
        {
            Assert.Throws<NotImplementedException>(() => target.Sort(SortingMode.Ascending));
        }

        [Test]
        public void VerifyLoadWeekdayMultiplicatorDefinitions()
        {
            _definitionSetCollection.Clear();
            _definitionSet1.AddDefinition(_dayOfWeekMultiplicatorDefinition);
            _definitionSetCollection.Add(_definitionSet1);
            _explorerPresenter.Model.FilteredDefinitionSetCollection = _definitionSetCollection;
            target.LoadMultiplicatorDefinitions();
            Assert.AreEqual(1,target.ModelCollection.Count);
        }

        [Test]
        public void VerifyRefreshView()
        {
            _definitionSetCollection.Clear();
            _definitionSet1.AddDefinition(_dayOfWeekMultiplicatorDefinition);
            _definitionSetCollection.Add(_definitionSet1);
            _explorerPresenter.Model.FilteredDefinitionSetCollection = _definitionSetCollection;

            target.RefreshView();

            Assert.IsTrue(target.ModelCollection.Count > 0);
        }

        [Test]
        public void VerifyBuildCopyString()
        {
            DateTime start = new DateTime(2010, 1, 1, 10, 0, 0);
            DateTime end = new DateTime(2010, 1, 1, 12, 0, 0);
            string expectedCopyString =
                string.Format(CultureInfo.CurrentCulture,
                              "Day Of Week\tTuesday\t01:01:00\t02:02:00\t\t\tO, OB1\r\nFrom - To\t\t\t\t{0} {1}\t{2} {3}\tO, OB1\r\n",
                              start.ToShortDateString(), start.ToShortTimeString(), end.ToShortDateString(),
                              end.ToShortTimeString());
            IList<IMultiplicatorDefinitionViewModel> viewModelCollection = new List<IMultiplicatorDefinitionViewModel>();
            IMultiplicatorDefinitionViewModel viewModel1 =
                new MultiplicatorDefinitionViewModel(_dayOfWeekMultiplicatorDefinition);
            IMultiplicatorDefinitionViewModel viewModel2 =
                new MultiplicatorDefinitionViewModel(_dateTimeMultiplicatorDefinition);
            viewModelCollection.Add(viewModel1);
            viewModelCollection.Add(viewModel2);

            string copyString = target.BuildCopyString(viewModelCollection);

            Assert.AreEqual(expectedCopyString, copyString);
        }
    }
}
