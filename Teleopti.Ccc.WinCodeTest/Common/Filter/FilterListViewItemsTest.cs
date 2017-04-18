using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;

namespace Teleopti.Ccc.WinCodeTest.Common.Filter
{
    [TestFixture]
    public class FilterListViewItemsTest
    {
        private IList<ListViewItem> _allItems;
        private ListViewItem l1;
        private ListViewItem l2;
        private ListViewItem l3;
        private ListViewItem l4;

        [SetUp]
        public void Setup()
        {
            _allItems = new List<ListViewItem>();
            l1 = new ListViewItem("Jultomte");
            l1.Name = "Jultomte";
            l1.SubItems.Add("Renenen");
            _allItems.Add(l1);

            l2 = new ListViewItem("Grisen");
            l2.Name = "Grisen";
            l2.SubItems.Add("Anders");
            _allItems.Add(l2);

            l3 = new ListViewItem("Jultomte");
            l3.Name = "Jultomte";
            l3.SubItems.Add("Polisen");
            l3.SubItems.Add("Thief");
            _allItems.Add(l3);

            l4 = new ListViewItem("Anders item");

            l4.Name = "Anders item";
            l4.SubItems.Add("Log objektet");
            _allItems.Add(l4);
        }

        [Test]
        public void ShouldFilterOnOneWord()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("risen");

            filtered.Should().Contain(l2);
            filtered.Should().Have.Count.EqualTo(1);
        }

        [Test]
        public void ShouldFilterOnTwoSeparateWords()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("jul enen");

            filtered.Should().Contain(l1);
            filtered.Should().Have.Count.EqualTo(1);
        }

        [Test]
        public void ShouldFilterOnThree()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("anders log");

            filtered.Should().Contain(l4);
            filtered.Should().Have.Count.EqualTo(1);
        }

        [Test]
        public void ShouldFilterOnAnders()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("Anders");

            filtered.Should().Contain(l2);
            filtered.Should().Contain(l4);
            filtered.Should().Have.Count.EqualTo(2);
        }

        [Test]
        public void ShouldFilterOnStuffWithTrailingSpace()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("Jul    ");

            filtered.Should().Contain(l1);
            filtered.Should().Contain(l3);
            filtered.Should().Have.Count.EqualTo(2);
        }
        
        [Test]
        public void ShouldFilterOnMoreSubitems()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("Thief");

            filtered.Should().Contain(l3);
            filtered.Should().Have.Count.EqualTo(1);
        }

        [Test]
        public void ShouldGiveAllBackIfEmptyFilter()
        {
            var filter = new FilterListViewItems(_allItems);
            var filtered = filter.Filter("");

            filtered.Should().Have.Count.EqualTo(4);
        }
    }
}
