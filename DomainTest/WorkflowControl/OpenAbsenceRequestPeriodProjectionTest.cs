using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
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
            _cultureInfo = new CultureInfo("sv-SE");
            _openAbsenceRequestPeriodExtractor = _mocks.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            _target = new OpenAbsenceRequestPeriodProjection(_openAbsenceRequestPeriodExtractor);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySimple()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod();
            absenceRequestOpenPeriod.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 30));
            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriod);

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);
            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
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
			    Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet).Return(new WorkflowControlSet("test WCS"));
				Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
			}

			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 01, 01);

			IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);
			Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
		}

        [Test]
        public void VerifyReturnsListWithDenyOnlyWhenEmptyListIn()
        {
            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test WCS"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);
            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess));
        }

        [Test]
        public void VerifyTwoIntersectingLayers()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodOne = new AbsenceRequestOpenDatePeriod();
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodTwo = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodOne.Period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 30));
            absenceRequestOpenPeriodTwo.Period = new DateOnlyPeriod(new DateOnly(2010, 4, 1), new DateOnly(2010, 6, 30));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodOne);
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodTwo);

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods).Repeat.AtLeastOnce();
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs")).Repeat.Twice();
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(absenceRequestOpenPeriodTwo.Period, projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));

            dateOnlyPeriod = new DateOnlyPeriod(2010, 03, 01, 2010, 04, 05);
            projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(2, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 3, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(absenceRequestOpenPeriodTwo.Period, projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
        }

        [Test]
        public void VerifyComplexCase1()
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
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod,_cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 6, 22), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 23, 2010, 6, 27), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 28, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));

        }

        [Test]
        public void VerifyComplexCase2()
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
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test WCS"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(3, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

        }

        [Test]
        public void VerifyComplexCase3()
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
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 03, 28)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 3, 29), new DateOnly(2010, 04, 04)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 4, 05), new DateOnly(2010, 06, 22)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27)), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 12, 31)), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));
        }

        [Test]
        public void VerifyComplexCase4()
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
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 08, 20, 2010, 08, 20);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 8, 20), new DateOnly(2010, 08, 20)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
        }

        [Test]
        public void VerifyComplexCaseOutsidePeriods()
        {
            AbsenceRequestOpenDatePeriod absenceRequestOpenPeriodMidsummer = new AbsenceRequestOpenDatePeriod();

            absenceRequestOpenPeriodMidsummer.Period = new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27));

            _openAbsenceRequestPeriods.Clear();
            _openAbsenceRequestPeriods.Add(absenceRequestOpenPeriodMidsummer);

            using (_mocks.Record())
            {
                Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(new WorkflowControlSet("test wcs"));
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(3, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 6, 22)), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 23), new DateOnly(2010, 6, 27)), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2010, 6, 28), new DateOnly(2010, 12, 31)), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));

            Assert.IsTrue(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess is DenyAbsenceRequest);
	        Assert.AreEqual(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonClosedPeriod", _cultureInfo),
	                        ((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess).DenyReason);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[1].AbsenceRequestProcess is PendingAbsenceRequest);
			Assert.IsTrue(projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess is DenyAbsenceRequest);
	        Assert.AreEqual(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonClosedPeriod", _cultureInfo),
	                        ((DenyAbsenceRequest) projectedOpenAbsenceRequestPeriods[2].AbsenceRequestProcess).DenyReason);
        }

        [Test]
        public void VerifyIfRequestPeriodIsNotOpen()
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
                Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
                      .Return(wcs).Repeat.Twice();
                Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate).Return(DateOnly.Today).Repeat.AtLeastOnce();
            }

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 01, 01, 2010, 12, 31);

            IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods = _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);

            Assert.AreEqual(5, projectedOpenAbsenceRequestPeriods.Count);
            Assert.AreEqual(new DateOnlyPeriod(2010, 1, 1, 2010, 5, 31), projectedOpenAbsenceRequestPeriods[0].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 1, 2010, 6, 22), projectedOpenAbsenceRequestPeriods[1].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 23, 2010, 6, 27), projectedOpenAbsenceRequestPeriods[2].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 6, 28, 2010, 8, 31), projectedOpenAbsenceRequestPeriods[3].GetPeriod(DateOnly.Today));
            Assert.AreEqual(new DateOnlyPeriod(2010, 9, 1, 2010, 12, 31), projectedOpenAbsenceRequestPeriods[4].GetPeriod(DateOnly.Today));
        }

        //[Test]
        //public void IfNotLayerFound()
        //{
        //    using (_mocks.Record())
        //    {
        //        Expect.Call(_openAbsenceRequestPeriodExtractor.AvailablePeriods).Return(_openAbsenceRequestPeriods);
        //        Expect.Call(_openAbsenceRequestPeriodExtractor.WorkflowControlSet)
        //              .Return(new WorkflowControlSet("test WCS"));
        //        Expect.Call(_openAbsenceRequestPeriodExtractor.ViewpointDate)
        //              .Return(DateOnly.Today)
        //              .Repeat.AtLeastOnce();
        //    }

        //    DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 05, 01, 2010, 05, 31);

        //    IList<IAbsenceRequestOpenPeriod> projectedOpenAbsenceRequestPeriods =
        //        _target.GetProjectedPeriods(dateOnlyPeriod, _cultureInfo);
        //    Assert.AreEqual(1, projectedOpenAbsenceRequestPeriods.Count);
        //    Assert.IsTrue(typeof (DenyAbsenceRequest).IsInstanceOfType(projectedOpenAbsenceRequestPeriods[0].AbsenceRequestProcess));
        //}
    }
}
