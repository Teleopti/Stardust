using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class OpenAbsenceRequestPeriodProjectionTest
    {
        private IOpenAbsenceRequestPeriodProjection _target;
        private IList<IAbsenceRequestOpenPeriod> _openAbsenceRequestPeriods;
        private MockRepository _mocks;
        private IOpenAbsenceRequestPeriodExtractor _openAbsenceRequestPeriodExtractor;
        private CultureInfo _cultureInfo;
        
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

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);
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

			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
			}

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 01, 01);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);
			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is PendingAbsenceRequest);
		}

        [Test]
        public void VerifyReturnsListWithDenyOnlyWhenEmptyListIn()
        {
			Assert.AreEqual(0, _openAbsenceRequestPeriods.Count); // make sure that the list is empty

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);
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

			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
			}

			var requestPeriod = new DateOnlyPeriod(2010, 06, 23, 2010, 06, 30);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

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

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod,_cultureInfo);

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

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

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

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

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

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 08, 20, 2010, 08, 20);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 8, 20), new DateOnly(2010, 08, 20)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
        }

        [Test]
        public void VerifyDenyReasonOutsidePeriod()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));
			absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

            Assert.AreEqual(3, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 22)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 12, 31)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

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
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodEarlier = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodEarlier.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));
			absenceRequestOpenPeriodEarlier.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodLater = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodLater.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 6, 30));
			absenceRequestOpenPeriodLater.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodLater);
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodEarlier);

			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
			}

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

			Assert.AreEqual(4, projectedOpenAbsenceRequestPeriods.Count);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 22)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 6, 30)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

			var expectedReason = string.Format(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", _cultureInfo), absenceRequestOpenPeriodEarlier.Period.ToShortDateString(_cultureInfo));
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.AreEqual(expectedReason, ((DenyAbsenceRequest)projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[1].AbsenceRequestProcess is DenyAbsenceRequest);
			Assert.IsFalse(projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess is DenyAbsenceRequest);
		}

		[Test]
		public void VerifyDenyReasonPeriodOpensLater()
		{
			AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

			absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));
			absenceRequestOpenPeriodMidsummer.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2010, 05, 30), new DateOnly(2010, 05, 31));

			_openAbsenceRequestPeriods.Clear();
			_openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(new Collection<IAbsenceRequestOpenPeriod>());
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(new DateOnly(2010, 05, 20)).Repeat.AtLeastOnce();
			}

			var requestPeriod = new DateOnlyPeriod(2010, 06, 25, 2010, 06, 26);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

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

			using (_mocks.Record())
			{
				Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(new Collection<IAbsenceRequestOpenPeriod>());
				Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(new DateOnly(2010, 05, 20)).Repeat.AtLeastOnce();
			}

			var requestPeriod = new DateOnlyPeriod(2010, 06, 25, 2010, 06, 26);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

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
            
            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.AllPeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

			var requestPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(requestPeriod, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 6, 22), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 23, 2010, 6, 27), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 28, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));
        }
    }
}
