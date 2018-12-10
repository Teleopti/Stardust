using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class ExplorerViewModelTest
    {
        private IExplorerViewModel _target;

        private IList<IMultiplicator> _multiplicatorCollection;
        private IList<IMultiplicatorDefinitionSet> _definitionSetCollection;
        private IList<IMultiplicatorDefinitionSet> _filteredDefinitionSetCollection;

        [SetUp]
        public void Setup()
        {
            _multiplicatorCollection = new List<IMultiplicator>(3);
            _multiplicatorCollection.Add(new Multiplicator(MultiplicatorType.OBTime));
            _multiplicatorCollection.Add(new Multiplicator(MultiplicatorType.Overtime));
            _multiplicatorCollection.Add(new Multiplicator(MultiplicatorType.OBTime));

            _definitionSetCollection = new List<IMultiplicatorDefinitionSet>(1);

            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("MyDefinition", 
                                                                                       MultiplicatorType.OBTime);
            definitionSet.AddDefinition(new DateTimeMultiplicatorDefinition(_multiplicatorCollection[0],
                                                                            DateOnly.MinValue, 
                                                                            DateOnly.MaxValue, 
                                                                            TimeSpan.FromHours(8), 
                                                                            TimeSpan.FromHours(12)));
            definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(_multiplicatorCollection[2],
                                                                            DayOfWeek.Monday,
                                                                            new TimePeriod(8, 0, 17, 0)));
            _definitionSetCollection.Add(definitionSet);

            _filteredDefinitionSetCollection = new List<IMultiplicatorDefinitionSet>(1);
            _filteredDefinitionSetCollection.Add(_definitionSetCollection[0]);

            _target = new ExplorerViewModel();
            _target.MultiplicatorCollection = _multiplicatorCollection;
            _target.DefinitionSetCollection = _definitionSetCollection;
            _target.FilteredDefinitionSetCollection = _filteredDefinitionSetCollection;
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.IsTrue(_target.MultiplicatorCollection.Count == 3);
            Assert.IsTrue(_target.DefinitionSetCollection.Count == 1);
            Assert.IsTrue(_target.FilteredDefinitionSetCollection.Count == 1);

            _target.SetSelectedDate(new DateOnly(2009, 02, 19));
            Assert.AreEqual(new DateOnly(2009, 02, 19), _target.SelectedDate.Value);

            _target.SetDefaultSegment(15);
            Assert.AreEqual(15, _target.DefaultSegment);

            _target.SetRightToLeft(true);
            Assert.AreEqual(true, _target.IsRightToLeft);

            Assert.AreEqual(7, _target.TranslatedWeekdaysCollection.Count);
        }


    }
}
