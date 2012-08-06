using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{

    public class RequestPreferencesPersonalSettingsTest
    {
        private RequestPreferencesPersonalSettings _target;
        private List<FilterBoxAdvancedFilter> _filterList ;

        [SetUp]
        public void Setup()
        {
            _target = new RequestPreferencesPersonalSettings();
            _filterList = new List<FilterBoxAdvancedFilter>();
            var f1 = new FilterBoxAdvancedFilter(new object(), new FilterAdvancedTupleItem(" ", " "),
                                                  new FilterAdvancedTupleItem("Equal", "="));
            _filterList.Add(f1);
        }

        [Test]
        public void VerifyMapFromNullTest()
        {
            _target.MapFrom(new RequestPreferences());
            Assert.IsFalse(_target.IsSettingExtracted());
        }

        [Test]
        public void VerifyMapFromNotNullTest()
        {
            IRequestPreferences rq;
            rq = new RequestPreferences();
            rq.RequestList = new List<FilterBoxAdvancedFilter>();
            _target.MapFrom(rq);
            Assert.IsTrue(_target.IsSettingExtracted());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Common.FilterAdvancedTupleItem.#ctor(System.String,System.Object)"), Test]
        public void VerifyMapToTest()
        {
            IRequestPreferences rq;
            rq = new RequestPreferences();
            rq.RequestList = new List<FilterBoxAdvancedFilter>();

            var f1 = new FilterBoxAdvancedFilter(new object(), new FilterAdvancedTupleItem(" ", " "),
                                                 new FilterAdvancedTupleItem("Equal", "="));
            rq.RequestList.Add(f1);
            _target.MapFrom(rq);

            var returningList = _target.MapTo();

            Assert.AreEqual(returningList.Count, 1);
        }

        [Test]
        public void VerifyReqPrefPropertyTest()
        {
            
           var rq = new RequestPreferences();
            rq.RequestList = _filterList;

            Assert.AreEqual(rq.RequestList , _filterList);
        }
    }
}
