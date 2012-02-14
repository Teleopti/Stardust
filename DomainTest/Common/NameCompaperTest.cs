using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class NameComparerTest
    {
        private NameComparer _target;
        private Name name1;
        private Name name2;
        private Name name3;
        private Name name4;
        private Name name5;
        private IList<Name> names;
        private CultureInfo _cultureInfo;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "Teleopti.Ccc.Domain.Common.NameComparer.#ctor"), Test]
        public void VerifyCompare()
        {
            name1 = new Name("Brigitta", "Svensson");
            name2 = new Name("Alma", "Svensson");
            name3 = new Name("Dalma", "Nordqist");
            name4 = new Name("Cecilia", "Nordqist");
            name5 = new Name();

            names = new List<Name> { name1, name2, name3, name4, name5 };

            _target = new NameComparer();

            IList<Name> sortedList = names.OrderBy(a => a, _target).ToList();
            Assert.AreEqual(sortedList.Count(), names.Count);
            Assert.AreEqual(sortedList[0], name5);
            Assert.AreEqual(sortedList[1], name4);
            Assert.AreEqual(sortedList[2], name3);
            Assert.AreEqual(sortedList[3], name2);
            Assert.AreEqual(sortedList[4], name1);
        }

        [Test]
        public void VerifyCompareSwedishAlphabetWithEnglishCulture()
        {
            name1 = new Name("Brigitta", "Ö");  // 4
            name2 = new Name("Alma", "Ä");      // 2
            name3 = new Name("Dalma", "A");     // 1
            name4 = new Name("Cecilia", "Å");   // 3
            name5 = new Name("Cecilia", "Y");   // 5

            names = new List<Name> { name1, name2, name3, name4, name5 };

            _cultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            _target = new NameComparer(_cultureInfo);

            IList<Name> sortedList = names.OrderBy(a => a, _target).ToList();
            Assert.AreEqual(sortedList.Count(), names.Count);
            Assert.AreEqual(sortedList[0], name3);
            Assert.AreEqual(sortedList[1], name2);
            Assert.AreEqual(sortedList[2], name4);
            Assert.AreEqual(sortedList[3], name1);
            Assert.AreEqual(sortedList[4], name5);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "Teleopti.Ccc.Domain.Common.NameComparer.#ctor"), Test]
        public void VerifyCompareSwedishAlphabetWithSwedishCulture()
        {

            _target = new NameComparer();

            name1 = new Name("Brigitta", "Ö");  // 5
            name2 = new Name("Alma", "Ä");      // 4
            name3 = new Name("Dalma", "A");     // 1
            name4 = new Name("Cecilia", "Å");   // 3
            name5 = new Name("Cecilia", "Y");   // 2

            names = new List<Name> { name1, name2, name3, name4, name5 };

            _cultureInfo = CultureInfo.CreateSpecificCulture("se-SE");
            _target = new NameComparer(_cultureInfo);

            IList<Name> sortedList = names.OrderBy(a => a, _target).ToList();
            Assert.AreEqual(sortedList.Count(), names.Count);
            Assert.AreEqual(sortedList[0], name3);
            Assert.AreEqual(sortedList[1], name5);
            Assert.AreEqual(sortedList[2], name4);
            Assert.AreEqual(sortedList[3], name2);
            Assert.AreEqual(sortedList[4], name1);
        }

    }
}
