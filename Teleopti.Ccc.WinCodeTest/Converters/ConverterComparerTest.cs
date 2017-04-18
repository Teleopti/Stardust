using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class ConverterComparerTest
    {
        private ConverterComparer _target;
        private MockRepository _mocks;
        private IValueConverter _converter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IValueConverter>();
            _target = new ConverterComparer(ListSortDirection.Ascending, "Day", _converter);
        }

        [Test]
        public void VerifyCompareAscending()
        {
            Expect.Call(_converter.Convert(1, null, null, CultureInfo.CurrentUICulture)).Return(1).Repeat.Any();
            Expect.Call(_converter.Convert(2, null, null, CultureInfo.CurrentUICulture)).Return(2).Repeat.Any();
            _mocks.ReplayAll();

            int result = _target.Compare(new DateTime(2008, 1, 1), new DateTime(2008, 1, 2));
            Assert.AreEqual(-1, result);
            result = _target.Compare(new DateTime(2008, 1, 2), new DateTime(2008, 1, 1));
            Assert.AreEqual(1, result);
            result = _target.Compare(new DateTime(2008, 1, 1), new DateTime(2008, 1, 1));
            Assert.AreEqual(0, result);
            _mocks.VerifyAll();
        }
        
        [Test]
        public void VerifyCompareNull()
        {
            Expect.Call(_converter.Convert(1, null, null, CultureInfo.CurrentUICulture)).Return(null);
            Expect.Call(_converter.Convert(2, null, null, CultureInfo.CurrentUICulture)).Return(null);
            _mocks.ReplayAll();

            int result = _target.Compare(new DateTime(2008, 1, 1), new DateTime(2008, 1, 2));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void VerifyCompareDescending()
        {
            _target = new ConverterComparer(ListSortDirection.Descending, "Day", _converter);

            Expect.Call(_converter.Convert(1, null, null, CultureInfo.CurrentUICulture)).Return(1).Repeat.Any();
            Expect.Call(_converter.Convert(2, null, null, CultureInfo.CurrentUICulture)).Return(2).Repeat.Any();
            _mocks.ReplayAll();

            int result = _target.Compare(new DateTime(2008, 1, 1), new DateTime(2008, 1, 2));
            Assert.AreEqual(1, result);
            result = _target.Compare(new DateTime(2008, 1, 2), new DateTime(2008, 1, 1));
            Assert.AreEqual(-1, result);
            result = _target.Compare(new DateTime(2008, 1, 1), new DateTime(2008, 1, 1));
            Assert.AreEqual(0, result);
            _mocks.VerifyAll();
        }
    }
}
