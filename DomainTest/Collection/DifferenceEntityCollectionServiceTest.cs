using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class DifferenceEntityCollectionServiceTest
    {
        private IDifferenceCollectionService<IPersonAbsence> target;
        private IPerson _person;
        private IScenario _scenario;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            target = new DifferenceEntityCollectionService<IPersonAbsence>();
        }

        [Test]
        public void VerifyOneCreatedOneDeletedOneChanged()
        {
            IPersonAbsence absOrg1 =
                PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                         new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            IPersonAbsence absOrg2 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                                              new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            absOrg2.SetId(Guid.NewGuid());

            IPersonAbsence absOrg2New = absOrg2.EntityClone();
            IPersonAbsence absNew = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                                             new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            IList<IPersonAbsence> org = new List<IPersonAbsence> { absOrg1, absOrg2 };
            IList<IPersonAbsence> current = new List<IPersonAbsence> { absOrg2New, absNew };

            IDifferenceCollection<IPersonAbsence> res = target.Difference(org, current);
            Assert.AreEqual(3, res.Count());
            Assert.IsTrue(res.Contains(new DifferenceCollectionItem<IPersonAbsence>(absOrg1, null)));
            Assert.IsTrue(res.Contains(new DifferenceCollectionItem<IPersonAbsence>(absOrg2, absOrg2New)));
            Assert.IsTrue(res.Contains(new DifferenceCollectionItem<IPersonAbsence>(null, absNew)));
        }

        [Test]
        public void VerifyNonModifiedItemIsNotReturned()
        {
            IPersonAbsence abs1 =
                PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                         new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            IPersonAbsence abs2 =
                PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                         new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            abs2.SetId(Guid.NewGuid());

            IList<IPersonAbsence> org = new List<IPersonAbsence> { abs1, abs2 };
            IList<IPersonAbsence> current = new List<IPersonAbsence> { abs1, abs2 };

            IDifferenceCollection<IPersonAbsence> res = target.Difference(org, current);
            Assert.AreEqual(0, res.Count());
        }

	    [Test, Ignore("Gift to Roger")]
	    public void NonModifiedPreferenceDayShouldNotBeReturned()
	    {
		    var preferenceDay =
			    new PreferenceDay(_person, new DateOnly(2016, 4, 17),
				    new PreferenceRestriction()
				    {
					    StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))
				    }).WithId();

		    var preferenceDayClone = (IPreferenceDay) preferenceDay.Clone();
			var org = new List<IPreferenceDay> { preferenceDayClone };
			var current = new List<IPreferenceDay> { preferenceDay };

			var testTarget = new DifferenceEntityCollectionService<IPreferenceDay>();

			IDifferenceCollection<IPreferenceDay> res = testTarget.Difference(org, current);
		    res.Count().Should().Be.EqualTo(0);
	    }
    }
}