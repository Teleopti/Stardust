using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.WinCode.Payroll.Wrappers;
using Teleopti.Interfaces.Domain;
using NUnit.Framework;

namespace Teleopti.Ccc.WinCodeTest.Payroll.Wrappers
{
    [TestFixture]
    public class DayOfWeekMultiplicatorDefinitionAdapterTest
    {
        
        #region Fields - Instance Members 

        private DayOfWeek _dayOfWeek;

        private IMultiplicator _multiplicator;

        private TimePeriod _period;
       
        private DayOfWeekMultiplicatorDefinition _dayOfWeekMultiplicatorDefinition;

        private DayOfWeekMultiplicatorDefinition _dayOfWeekMultiplicatorDefinition2;

        private string _startTime;
        private string _endTime;
        #endregion

        #region Methods - Instance Members

        #region  Methods - Instance Members - Test Methods

        [Test]
        public void VerifyCanAssignSelectedDayOfWeek()
        {
            DayOfWeekMultiplicatorDefinitionAdapter adapter = new DayOfWeekMultiplicatorDefinitionAdapter(_dayOfWeekMultiplicatorDefinition);

            adapter.SelectedDayOfWeek = DayOfWeek.Friday;

            Assert.AreEqual(DayOfWeek.Friday, adapter.SelectedDayOfWeek);
        }

        [Test]
        public void VerifyCanAssignSMultiplicatorType()
        {
            DayOfWeekMultiplicatorDefinitionAdapter adapter = new DayOfWeekMultiplicatorDefinitionAdapter(_dayOfWeekMultiplicatorDefinition);

            adapter.MultiplicatorType = MultiplicatorType.Overtime;

            Assert.AreEqual(MultiplicatorType.Overtime, adapter.MultiplicatorType);
        }

        //[Test]
        //public void VerifyCanGetOrderIndex()
        //{
        //    //DayOfWeekMultiplicatorDefinitionAdapter adapter = new DayOfWeekMultiplicatorDefinitionAdapter(_dayOfWeekMultiplicatorDefinition);

        //    //Assert.AreEqual(_dayOfWeekMultiplicatorDefinition.OrderIndex, adapter.OrederIndex);
        //}

        [Test]
        public void VerifyCanAssignStartTime()
        {
            DayOfWeekMultiplicatorDefinitionAdapter adapter = new DayOfWeekMultiplicatorDefinitionAdapter(_dayOfWeekMultiplicatorDefinition);

            adapter.StartTime = _startTime;

            TimeSpan adapterStartTimeParser;
            TimeHelper.TryParse(adapter.StartTime, out adapterStartTimeParser);

            TimeSpan inputStartTimeParser;
            TimeHelper.TryParse(_startTime, out inputStartTimeParser);


            Assert.AreEqual(adapterStartTimeParser, inputStartTimeParser);
        }

        [Test]
        public void VerifyCanAssignEndTime()
        {
            DayOfWeekMultiplicatorDefinitionAdapter adapter = new DayOfWeekMultiplicatorDefinitionAdapter(_dayOfWeekMultiplicatorDefinition);

            adapter.EndTime = _endTime;

            TimeSpan adapterEndTimeParser;
            TimeHelper.TryParse(adapter.EndTime, out adapterEndTimeParser);

            TimeSpan inputEndTimeParser;
            TimeHelper.TryParse(_endTime, out inputEndTimeParser);


            Assert.AreEqual(adapterEndTimeParser, inputEndTimeParser);
        }


        [Test]
        public void VerifyCanParseDefinitionCollection()
        {
         
            IList<DayOfWeekMultiplicatorDefinition> list = new List<DayOfWeekMultiplicatorDefinition>();

            list.Add(_dayOfWeekMultiplicatorDefinition);
            list.Add(_dayOfWeekMultiplicatorDefinition2);

            Assert.AreEqual(list[0], DayOfWeekMultiplicatorDefinitionAdapter.Parse(list)[0].ContainedEntity);
        }

        [Test]
        public void VerifyCanParseSingleDefinition()
        {
            Assert.AreEqual(_dayOfWeekMultiplicatorDefinition, DayOfWeekMultiplicatorDefinitionAdapter.Parse(_dayOfWeekMultiplicatorDefinition).ContainedEntity);
        }

        #endregion

        #region Methods - Instance Members - Setup and Teardown Methods

        [SetUp]
        public void Setup()
        {
            _dayOfWeek = DayOfWeek.Monday;
            _period = new TimePeriod(new TimeSpan(8,0,0) , new TimeSpan(8,30,0));
            _multiplicator = new Multiplicator(MultiplicatorType.OBTime);

            _dayOfWeekMultiplicatorDefinition = new DayOfWeekMultiplicatorDefinition(_multiplicator, _dayOfWeek, _period);
            _dayOfWeekMultiplicatorDefinition2 = new DayOfWeekMultiplicatorDefinition(_multiplicator,DayOfWeek.Wednesday , _period);
            _startTime = "8:05";
            _endTime = "9:35";
        }

        #endregion

        #endregion
    }
}
