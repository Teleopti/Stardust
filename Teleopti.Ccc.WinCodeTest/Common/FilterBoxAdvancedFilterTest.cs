﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class FilterBoxAdvancedFilterTest
    {
        private FilterBoxAdvancedFilter _filterBoxAdvancedFilter;
        FilterAdvancedTupleItem _filterOperand;
        FilterAdvancedTupleItem _criteria;
        private FilterAdvancedTupleItem _value;

        [SetUp]
        public void Setup()
        {
            _filterOperand = new FilterAdvancedTupleItem("=","equals");
            _criteria = new FilterAdvancedTupleItem("criteria", "criteria");
            _filterBoxAdvancedFilter = new FilterBoxAdvancedFilter("filterOn", _filterOperand, _criteria);
            _value = new FilterAdvancedTupleItem("Equal", "=");
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("filterOn", _filterBoxAdvancedFilter.FilterOn.ToString());
            Assert.AreEqual(_criteria, _filterBoxAdvancedFilter.FilterCriteria);
            Assert.AreEqual(_filterOperand, _filterBoxAdvancedFilter.FilterOperand);
        }

        [Test]
        public void VerifyPropertiesValues()
        {
            _filterBoxAdvancedFilter = new FilterBoxAdvancedFilter("filterOn", _filterOperand, _criteria, _value);
            Assert.AreEqual("filterOn", _filterBoxAdvancedFilter.FilterOn.ToString());
            Assert.AreEqual(_criteria, _filterBoxAdvancedFilter.FilterCriteria);
            Assert.AreEqual(_filterOperand, _filterBoxAdvancedFilter.FilterOperand);
            Assert.AreEqual(_value , _filterBoxAdvancedFilter.FilterValue );

        }
    }
}
