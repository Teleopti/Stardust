using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class FilterAdvancedSettingTest
    {
        FilterAdvancedSetting _filterAdvancedSetting;

        [SetUp]
        public void Setup()
        {
            _filterAdvancedSetting = new FilterAdvancedSetting(new FilterAdvancedTupleItem("text", "filterOn"), new List<FilterAdvancedTupleItem>(), FilterCriteriaType.Time, new List<FilterAdvancedTupleItem>());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("text", _filterAdvancedSetting.Text);
            Assert.AreEqual("filterOn", ((FilterAdvancedTupleItem)_filterAdvancedSetting.FilterOn).Value.ToString());
            Assert.IsNotNull(_filterAdvancedSetting.FilterOperands);
            Assert.AreEqual(FilterCriteriaType.Time, _filterAdvancedSetting.FilterCriteriaType);
            Assert.IsNotNull(_filterAdvancedSetting.FilterCriteriaList);
        }
    }
}
