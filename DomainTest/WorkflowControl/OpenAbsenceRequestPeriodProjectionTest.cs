using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[DomainTest]
    public class OpenAbsenceRequestPeriodProjectionTest
    {
        private IOpenAbsenceRequestPeriodProjection _target;
        private IList<IAbsenceRequestOpenPeriod> _openAbsenceRequestPeriods;
        private MockRepository _mocks;
        private IOpenAbsenceRequestPeriodExtractor _openAbsenceRequestPeriodExtractor;
        private CultureInfo _cultureInfo;

	    public MutableNow Now;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _openAbsenceRequestPeriods = new List<IAbsenceRequestOpenPeriod>();
	        _cultureInfo = CultureInfoFactory.CreateEnglishCulture();
            _openAbsenceRequestPeriodExtractor = _mocks.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            _target = new OpenAbsenceRequestPeriodProjection(_openAbsenceRequestPeriodExtractor);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyRequestPeriodInsidePeriod()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod();
            absenceRequestOpenPeriod.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 30));
            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);
            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is PendingAbsenceRequest);
        }

		[Test]
		public void ShouldHandleOpenOneDay()
		{
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriod.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 1));
			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 01, 01);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);
			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is PendingAbsenceRequest);
		}

        [Test]
        public void VerifyReturnsListWithDenyOnlyWhenEmptyListIn()
        {
			Assert.AreEqual(0, _openAbsenceRequestPeriods.Count); // make sure that the list is empty

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);
            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess));
        }

		[Test]
		public void VerifyOverlappingPeriodsShouldBeAccepted()
		{
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodEarlier = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodEarlier.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));
			absenceRequestOpenPeriodEarlier.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodLater = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodLater.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 25), new DateOnly(2010, 6, 30));
			absenceRequestOpenPeriodLater.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodEarlier);
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodLater);

			setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 06, 23, 2010, 06, 30);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(2, projectedOpenAbsenceRequestPeriods.Count);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 24)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 25), new DateOnly(2010, 6, 30)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));

			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[1].AbsenceRequestProcess is DenyAbsenceRequest);
		}


        [Test]
        public void VerifyProjectionWithShortPeriodsInsideLong()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodWholeYear = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodSummer = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodWholeYear.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 12, 31));
            absenceRequestOpenPeriodSummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 1), new DateOnly(2010, 8, 31));
            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodWholeYear);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodSummer);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod,_cultureInfo, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 6, 22), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 23, 2010, 6, 27), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 28, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));

        }

        [Test]
		public void VerifyProjectionWithShortestPeriodHidden()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodWholeYear = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodSummer = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodWholeYear.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 12, 31));
            absenceRequestOpenPeriodSummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 1), new DateOnly(2010, 8, 31));
            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodWholeYear);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);  // Midsummer days now hidden behind Summer period.
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodSummer);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

            Assert.AreEqual(3, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

        }

        [Test]
		public void VerifyProjectionWithTwoSeparatePeriods()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodWholeEaster = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodWholeEaster.Period = new DateOnlyPeriod(new DateOnly(2010, 3, 29), new DateOnly(2010, 04, 04));
            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodWholeEaster);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 03, 28)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 3, 29), new DateOnly(2010, 04, 04)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 4, 05), new DateOnly(2010, 06, 22)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27)), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 12, 31)), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));
        }

        [Test]
		public void VerifyProjectionShortPeriodInsideLongRequestingForShort()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodWholeAugust = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodCancerDisc = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodWholeAugust.Period = new DateOnlyPeriod(new DateOnly(2010, 8, 1), new DateOnly(2010, 8, 31));
            absenceRequestOpenPeriodCancerDisc.Period = new DateOnlyPeriod(new DateOnly(2010, 8, 20), new DateOnly(2010, 8, 20));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodWholeAugust);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodCancerDisc);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 08, 20, 2010, 08, 20);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 8, 20), new DateOnly(2010, 08, 20)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
        }

        [Test]
        public void VerifyDenyReasonOutsidePeriod()
        {
	        var today = DateOnly.Today;
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod = new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5));
			absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(today.AddDays(-10), today.AddDays(10));

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

            Assert.AreEqual(3, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(today.AddDays(-10), today.AddDays(-6)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(today.AddDays(6), today.AddDays(10)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

			var expectedReason = string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", _cultureInfo), absenceRequestOpenPeriodMidsummer.Period.ToShortDateString(_cultureInfo));
            Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason, ((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[1].AbsenceRequestProcess is DenyAbsenceRequest);
            Assert.IsTrue(projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason, ((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess).DenyReason);
        }

		[Test]
		public void VerifyDenyReasonWithTwoOutsidePeriodsEarlierShouldDisplayed()
		{
			var today = DateOnly.Today;
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodEarlier = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodEarlier.OpenForRequestsPeriod = new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5));
			absenceRequestOpenPeriodEarlier.Period = new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5));

			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodLater = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodLater.OpenForRequestsPeriod = new DateOnlyPeriod(today.AddDays(6), today.AddDays(10));
			absenceRequestOpenPeriodLater.Period = new DateOnlyPeriod(today.AddDays(6), today.AddDays(10));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodLater);
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodEarlier);

			setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(today.AddDays(-10), today.AddDays(15));

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(4, projectedOpenAbsenceRequestPeriods.Count);
			Assert.AreEqual(new DateOnlyPeriod(today.AddDays(-10), today.AddDays(-6)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
			Assert.AreEqual(new DateOnlyPeriod(today.AddDays(-5), today.AddDays(5)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
			Assert.AreEqual(new DateOnlyPeriod(today.AddDays(6), today.AddDays(10)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

			var expectedReason = string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", _cultureInfo), absenceRequestOpenPeriodEarlier.Period.ToShortDateString(_cultureInfo));
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason, ((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[1].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess is DenyAbsenceRequest);
		}

	    [Test]
	    public void VerifyDenyResonClosedPeriod()
	    {
			var today = new DateOnly(2017, 1, 25);
			Now.Is(today.Date);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriod.Period = new DateOnlyPeriod(today.AddDays(-5), today.AddDays(-1));
			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

		    setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(today.AddDays(2), today.AddDays(4));

			var projectedPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			var expected = string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonClosedPeriod", _cultureInfo)
					, absenceRequestOpenPeriod.Period.ToShortDateString(_cultureInfo));

			Assert.AreEqual(expected, ((DenyAbsenceRequest)projectedPeriods[0].AbsenceRequestProcess).DenyReason);
		}

		[Test]
		public void VerifyDenyReasonPeriodOpensLater()
		{
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));
			absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

			setOpenAbsenceRequestPeriodExtractor(new Collection<IAbsenceRequestOpenPeriod>(), new DateOnly(2010, 05, 20));


			var requestPeriod = new DateOnlyPeriod(2010, 06, 25, 2010, 06, 26);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			var expected =
				string.Format(
					UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonPeriodOpenAfterSendRequest", _cultureInfo),
					absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod.StartDate.ToShortDateString(_cultureInfo));

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expected, ((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
		}

		[Test]
		public void VerifyDenyReasonClosestPeriodOpensLater()
		{
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 12, 31));
			absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodChristmas = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodChristmas.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 12, 31));
			absenceRequestOpenPeriodChristmas.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 09, 30), new DateOnly(2010, 10, 31));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodChristmas);

			setOpenAbsenceRequestPeriodExtractor(new Collection<IAbsenceRequestOpenPeriod>(), new DateOnly(2010, 05, 20));


			var requestPeriod = new DateOnlyPeriod(2010, 06, 25, 2010, 06, 26);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			var expected =
				string.Format(
					UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonPeriodOpenAfterSendRequest", _cultureInfo),
					absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod.StartDate.ToShortDateString(_cultureInfo));

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expected, ((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
		}

        [Test]
        public void VerifyIfRequestPeriodClosed()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodWholeYear = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodSummer = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodWholeYear.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 12, 31));
            absenceRequestOpenPeriodSummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 1), new DateOnly(2010, 8, 31));
            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodWholeYear);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodSummer);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

            var period1 = new AbsenceRequestOpenDatePeriod();
            period1.Period = new DateOnlyPeriod(new DateOnly(2011, 1, 1), new DateOnly(2011, 6, 30));
            var period2 = new AbsenceRequestOpenDatePeriod();
            period1.Period = new DateOnlyPeriod(new DateOnly(2011, 1, 1), new DateOnly(2011, 6, 30));

            var test = new AbsenceRequestOpenDatePeriod();
            test.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2011,01,01), new DateOnly(2011,6,30) );
            
            var wcs = new WorkflowControlSet("test");
            wcs.AddOpenAbsenceRequestPeriod(test);
            wcs.AddOpenAbsenceRequestPeriod(period2);

	        setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 6, 22), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 23, 2010, 6, 27), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 28, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));
        }

	    [Test]
	    public void ShouldGetNoPeriodDenyReasonForSpecifiedCulture()
	    {
		    var today = DateOnly.Today;
		    var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod();
		    absenceRequestOpenPeriod.Period = new DateOnlyPeriod(today.AddDays(-30), today.AddDays(30));
		    absenceRequestOpenPeriod.OpenForRequestsPeriod = absenceRequestOpenPeriod.Period;
			_openAbsenceRequestPeriods.Clear();
		    _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor();

		    var dateCulture = CultureInfo.GetCultureInfo("sv-SE");
		    var languageCulture = CultureInfo.GetCultureInfo("zh-CN");
		    var requestPeriod = new DateOnlyPeriod(today.AddDays(-60), today.AddDays(-40));

		    var projectedPeriods = _target.GetProjectedPeriods(requestPeriod, dateCulture, languageCulture);

		    var expected =
			    string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", languageCulture)
				    , absenceRequestOpenPeriod.Period.ToShortDateString(dateCulture));

		    Assert.AreEqual(expected, ((DenyAbsenceRequest) projectedPeriods[0].AbsenceRequestProcess).DenyReason);
	    }

	    [Test]
	    public void ShouldReturnDenyFromToPeriodAndAutoGrantRollingPeriod()
	    {
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var openForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 31);
			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(new AbsenceRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, 1),
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				OpenForRequestsPeriod = openForRequestsPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			_openAbsenceRequestPeriods.Add(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 12, 24, 2016, 12, 25),
				OpenForRequestsPeriod = openForRequestsPeriod,
				AbsenceRequestProcess = new DenyAbsenceRequest()
			});

		    setOpenAbsenceRequestPeriodExtractor(viewpointDate: new DateOnly(2016, 12, 23));

			var dateCulture = CultureInfo.GetCultureInfo("en-US");
			var languageCulture = CultureInfo.GetCultureInfo("en-US");
			var requestPeriod = new DateOnlyPeriod(2016, 12, 24, 2016, 12, 24);

			var projectedPeriods = _target.GetProjectedPeriods(requestPeriod, dateCulture, languageCulture);

			Assert.AreEqual(2, projectedPeriods.Count);
			Assert.IsTrue(projectedPeriods.Any(p => p.AbsenceRequestProcess is DenyAbsenceRequest));
			Assert.IsTrue(projectedPeriods.Any(p => p.AbsenceRequestProcess is GrantAbsenceRequest));
		}

		[DatapointSource]
		public DateOnly[] OpenPeriodStartDates = { DateOnly.Today.AddDays(1), DateOnly.Today.AddDays(-10) };

		[Theory]
		public void ShouldNotSuggestRequestPeriodWhenRequestIsOutsideOpenPeriodAndAutoIsOn(DateOnly openPeriodStartDate)
		{
			var today = DateOnly.Today;
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new DenyAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 1),
				OpenForRequestsPeriod = new DateOnlyPeriod(openPeriodStartDate, today.AddDays(6)),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(today.AddDays(10), today.AddDays(20));

			var projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.AreEqual(new DateOnlyPeriod(today.AddDays(10), today.AddDays(20)),
				projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));

			var expectedReason = Resources.RequestDenyReasonAutodeny;
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason,
				((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
		}

		[Theory]
		public void ShouldSuggestRequestPeriodWhenRequestIsOutsideOpenPeriodWithAtLeastOneNonAutoDenyPeriod(DateOnly openPeriodStartDate)
		{
			var today = DateOnly.Today;
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			var nonAutoDenyAbsenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new PendingAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 5),
				OpenForRequestsPeriod = new DateOnlyPeriod(openPeriodStartDate, today.AddDays(6)),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			var autoDenyAbsenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new DenyAbsenceRequest(),
				Period = new DateOnlyPeriod(today.AddDays(1), today.AddDays(2)),
				OpenForRequestsPeriod = new DateOnlyPeriod(openPeriodStartDate, today.AddDays(6)),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(nonAutoDenyAbsenceRequestOpenPeriod);
			_openAbsenceRequestPeriods.Add(autoDenyAbsenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor();

			var requestPeriod = new DateOnlyPeriod(today.AddDays(6), today.AddDays(6));

			var projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.AreEqual(new DateOnlyPeriod(today.AddDays(6), today.AddDays(6)),
				projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));

			var expectedReason = Resources.RequestDenyReasonAutodeny;
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Console.WriteLine(((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
			Assert.AreNotEqual(expectedReason,
				((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
		}

		[Test,SetCulture("en-US")]
		public void ShouldSuggestRequestPeriodExceptDenyDays()
		{
			Now.Is(new DateTime(2017,1,1));

			var openPeriodStartDate = new DateOnly(2017, 1, 1);
			var openPeriodEndDate = new DateOnly(2017, 1, 31);
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			var nonAutoDenyAbsenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new PendingAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 5),
				OpenForRequestsPeriod = new DateOnlyPeriod(openPeriodStartDate, openPeriodEndDate),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			var denyDays = new[] { new DateOnly(2017, 1, 26), new DateOnly(2017, 1, 27) };
			var autoDenyAbsenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new DenyAbsenceRequest(),
				Period = new DateOnlyPeriod(denyDays[0], denyDays[1]),
				OpenForRequestsPeriod = new DateOnlyPeriod(openPeriodStartDate, openPeriodEndDate),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(nonAutoDenyAbsenceRequestOpenPeriod);
			_openAbsenceRequestPeriods.Add(autoDenyAbsenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor(viewpointDate:new DateOnly(2017, 1, 25));

			var requestPeriod = new DateOnlyPeriod(openPeriodEndDate, openPeriodEndDate);

			var projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);

			var expectedReason =
				"Your absence request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 25/01/2017 - 25/01/2017,28/01/2017 - 30/01/2017.";
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			var actualReason = ((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason;
			Assert.AreEqual(expectedReason, actualReason);
		}

		[Theory]
		public void ShouldNotSuggestRequestPeriodWhenRequestIsOutsideOpenPeriodAndAutoGrantIsOn(DateOnly openPeriodStartDate)
		{
			var today = new DateOnly(2017, 2, 13);
			var absence = AbsenceFactory.CreateAbsence("holiday").WithId();
			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 5),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2017, 1, 1), new DateOnly(2017, 1, 12)),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator()
			};

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

			setOpenAbsenceRequestPeriodExtractor(viewpointDate: today);

			var requestPeriod = new DateOnlyPeriod(new DateOnly(2017, 2, 24), new DateOnly(2017, 2, 24));

			var projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo, _cultureInfo);

			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);

			var expectedReason = Resources.RequestDenyReasonClosedPeriod;
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason,
				((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
		}

		private void setOpenAbsenceRequestPeriodExtractor(Collection<IAbsenceRequestOpenPeriod> availablePeriods = null, DateOnly? viewpointDate = null)
		{
			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(availablePeriods ?? _openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods).Repeat.AtLeastOnce();
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(viewpointDate ?? DateOnly.Today).Repeat.AtLeastOnce();
			}
		}
	}
}
