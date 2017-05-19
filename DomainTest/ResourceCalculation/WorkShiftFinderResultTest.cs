using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class WorkShiftFinderResultTest
    {
        private IPerson _person;
        private DateOnly _theDate;
        private WorkShiftFinderResult _target;
        private WorkShiftFilterResult _filter;

        [SetUp]
        public void Setup()
        {
            _person = new Person().WithId();
            _theDate = new DateOnly(2008, 1, 1);
            _target = new WorkShiftFinderResult(_person,_theDate);
            _filter = new WorkShiftFilterResult("hej", 100, 50);
        }
        
        [Test]
        public void VerifyCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Successful = true;
            
            Assert.AreEqual(0, _target.FilterResults.Count);

			_target.AddFilterResults(_filter);

            Assert.AreEqual(1,_target.FilterResults.Count);
            Assert.AreEqual(true,_target.Successful);
            Assert.AreEqual(_person,_target.Person);
            Assert.AreEqual(_theDate, _target.ScheduleDate);
            Assert.IsNotNull(_target.PersonName);

            var expectedKey = new Tuple<Guid,DateOnly>(_person.Id.Value, _target.ScheduleDate);
            Assert.AreEqual(expectedKey, _target.PersonDateKey);
        }
		
        [Test]
        public void VerifyFilterProperties()
        {
            Assert.AreEqual("hej", _filter.Message);
            Assert.AreEqual(100, _filter.WorkShiftsBefore);
            Assert.AreEqual(50, _filter.WorkShiftsAfter);

        }

        [Test]
        public void VerifyHolderList()
        {
            WorkShiftFinderResultHolder holder = new WorkShiftFinderResultHolder();
			_target.AddFilterResults(_filter);
            holder.AddResults(new List<WorkShiftFinderResult>{_target}, DateTime.Now);

            Assert.IsNotNull(holder.GetResults());
        }

        [Test]
        public void VerifyHolderListFilter()
        {
            WorkShiftFinderResultHolder holder = new WorkShiftFinderResultHolder();
			_target.AddFilterResults(_filter);
            holder.AddResults(new List<WorkShiftFinderResult> { _target }, DateTime.Now);

            DateOnly theDate2 = new DateOnly(2008, 1, 2);
            var result2 = new WorkShiftFinderResult(_person, theDate2);
            holder.AddResults(new List<WorkShiftFinderResult> { result2 }, theDate2.Date);

            Assert.IsNotNull(holder.GetResults(true, false));
            Assert.AreEqual(1,holder.GetResults(true, false).Count);
            Assert.AreEqual(2, holder.GetResults(false, false).Count);
        }

        [Test]
        public void VerifyHolderListFilterSuccess()
        {
            WorkShiftFinderResultHolder holder = new WorkShiftFinderResultHolder();
			_target.AddFilterResults(_filter);
            _target.Successful = false;
            holder.AddResults(new List<WorkShiftFinderResult> { _target }, DateTime.Now);

            DateOnly theDate2 = new DateOnly(2008, 1, 2);
            var result2 = new WorkShiftFinderResult(_person, theDate2) {Successful = true};

            holder.AddResults(new List<WorkShiftFinderResult> { result2 }, theDate2.Date);

            Assert.IsNotNull(holder.GetResults(true, false));
            Assert.AreEqual(1, holder.GetResults(true, false).Count);
            Assert.AreEqual(2, holder.GetResults(false, false).Count);
            Assert.AreEqual(1, holder.GetResults(false,true).Count);
            Assert.AreEqual(0, holder.GetResults(true, true).Count);

        }

        [Test]
        public void VerifyLastSuccessful()
        {
            WorkShiftFinderResultHolder holder = new WorkShiftFinderResultHolder();
			_target.AddFilterResults(_filter);
            _target.Successful = false;
            holder.AddResults(new List<WorkShiftFinderResult> { _target }, DateTime.Now);

            DateOnly theDate2 = new DateOnly(2008, 1, 2);
            var result2 = new WorkShiftFinderResult(_person, theDate2) {Successful = true};

            holder.AddResults(new List<WorkShiftFinderResult> { result2 }, theDate2.Date);

            Assert.IsTrue(holder.LastResultIsSuccessful);

            DateOnly theDate3 = new DateOnly(2008, 1, 3);
            WorkShiftFinderResult result3 = new WorkShiftFinderResult(_person, theDate3) {Successful = false};

            holder.AddResults(new List<WorkShiftFinderResult> { result3 }, theDate3.Date);

            Assert.IsFalse(holder.LastResultIsSuccessful);

        }

		[Test]
		public void EqualsShouldReturnFalseIfNull()
		{
			Assert.IsFalse(_target.Equals(null));
		}

		[Test]
		public void EqualsShouldReturnTrueIfSame()
		{
			Assert.IsTrue(_target.Equals(_target));
		}

		[Test]
		public void ShouldNotAddResultIfItExistsBefore()
		{
			var holder = new WorkShiftFinderResultHolder();
			holder.AddResults(new List<WorkShiftFinderResult>{  _target},DateTime.Now);
			_target = new WorkShiftFinderResult(_person, _theDate);
			holder.AddResults(new List<WorkShiftFinderResult>{  _target},DateTime.Now);

			Assert.That(holder.GetResults().Count,Is.EqualTo(1));
		}

		[Test]
		public void CanAddFilterToFinderResultOnHolder()
		{
			var holder = new WorkShiftFinderResultHolder();
			holder.AddResults(new List<WorkShiftFinderResult> { _target }, DateTime.Now);
			Assert.That(holder.GetResults().Count, Is.EqualTo(1));
			var result = holder.GetResults()[0];
			Assert.That(result.FilterResults.Count, Is.EqualTo(0));
			holder.AddFilterToResult(_person, _theDate, "This is an example");
			Assert.That(holder.GetResults().Count, Is.EqualTo(1));
			Assert.That(result.FilterResults.Count, Is.EqualTo(1));
			Assert.That(result.FilterResults[0].Message, Is.EqualTo("This is an example"));
		}

		[Test]
		public void CanAddFilterResultToNewPersonOrDate()
		{
			var holder = new WorkShiftFinderResultHolder();
			Assert.That(holder.GetResults().Count, Is.EqualTo(0));
			holder.AddFilterToResult(_person, _theDate, "This is an example");
			Assert.That(holder.GetResults().Count, Is.EqualTo(1));
			var result = holder.GetResults()[0];
			Assert.That(result.FilterResults[0].Message, Is.EqualTo("This is an example"));
			
			holder.AddFilterToResult(_person, _theDate.AddDays(1), "This is on another date");
			result = holder.GetResults()[1];
			Assert.That(holder.GetResults().Count, Is.EqualTo(2));
			Assert.That(result.FilterResults[0].Message, Is.EqualTo("This is on another date"));
			
			holder.AddFilterToResult(_person, _theDate.AddDays(1),"");
			Assert.That(result.Successful, Is.False);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.ResourceCalculation.WorkShiftFinderResultHolder.AddFilterToResult(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String)"), Test]
		public void CanSetResultSuccessfulViaHolder()
		{
			var holder = new WorkShiftFinderResultHolder();
			holder.AddFilterToResult(_person, _theDate, "This is an example");
			var result = holder.GetResults()[0];
			Assert.That(result.Successful, Is.False);
			holder.SetResultSuccessful(_person, _theDate,"");
			Assert.That(result.Successful, Is.True);
		}

		[Test]
		public void HolderCreatesNewResultWhenSettingSuccessfulToNewPersonOrDate()
		{
			var holder = new WorkShiftFinderResultHolder();
			holder.SetResultSuccessful(_person, _theDate,"");
			var result = holder.GetResults()[0];
			Assert.That(result.Successful, Is.True);
			Assert.That(result.FilterResults.Count, Is.EqualTo(0));
		}

		[Test]
		public void HolderCreatesNewResultAndFilterWhenSettingSuccessfulWithMessage()
		{
			var holder = new WorkShiftFinderResultHolder();
			holder.SetResultSuccessful(_person, _theDate, "NewMessage");
			var result = holder.GetResults()[0];
			Assert.That(result.Successful, Is.True);
			Assert.That(result.FilterResults.Count,Is.EqualTo(1));
		}

		[Test]
		public void ShouldNotHaveFilterResultsWithSameKey()
		{
			var filter1 = new WorkShiftFilterResult("Message 1", 0, 0);
			var filter2 = new WorkShiftFilterResult("Message 1", 0, 0);

			var result = new WorkShiftFinderResult(_person, _theDate);
			result.AddFilterResults(filter1);
			result.AddFilterResults(filter2);
			Assert.That(result.FilterResults.Count, Is.EqualTo(1));
		}

		[Test]
		public void EqualsOnFilterShouldReturnFalseIfNull()
		{
			Assert.IsFalse(_filter.Equals(null));
		}

		[Test]
		public void EqualsOnFilterShouldReturnTrueIfSame()
		{
			Assert.IsTrue(_filter.Equals(_filter));
		}
    }
}
