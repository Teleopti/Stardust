using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class UnorderedEventsTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutInOut_OutIn")]
		public void ShouldHandleAdherenceChanges(TestCase testCase)
		{
			testCase.Events.ForEach(e => Target.Handle((dynamic)e));
			Persister.Get(testCase.Site).Count.Should().Be(1);
		}

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutDeleteInOut")]
		public void ShouldHandleDeletions(TestCase testCase)
		{
			testCase.Events.ForEach(e => Target.Handle((dynamic)e));
			Persister.Get(testCase.Site).Count.Should().Be(0);
		}

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutTerminatedInOut")]
		public void ShouldHandleTerminations(TestCase testCase)
		{
			testCase.Events.ForEach(e => Target.Handle((dynamic)e));
			Persister.Get(testCase.Site).Count.Should().Be(0);
		}

		[Test]
		[TestCaseSource(typeof(UnorderedEventsTest), "OutSiteChangeInOut")]
		public void ShouldHandleSiteChanges(TestCase testCase)
		{
			testCase.Events.ForEach(e => Target.Handle((dynamic)e));
			Persister.Get(testCase.OriginSite).Count.Should().Be(0);
			Persister.Get(testCase.DestinationSite).Count.Should().Be(1);
		}

		public static IEnumerable OutInOut_OutIn
		{
			get
			{
				var personId1 = Guid.NewGuid();
				var personId2 = Guid.NewGuid();
				var siteId = Guid.NewGuid();
				var events = new List<IEvent>
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:00".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId1,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:04".Utc()
					},

					new PersonOutOfAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId2,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					}
				};
				return events
					.Permutations()
					.TestCases(null, i => i.Site = siteId);
			}
		}

		public static IEnumerable OutDeleteInOut
		{
			get
			{
				var personId = Guid.NewGuid();
				var siteId = Guid.NewGuid();
				var events = new IEvent[]
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonDeletedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-18 12:04".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					},
				};

				return events
					.Permutations()
					.Where(p => !(p.First() is PersonDeletedEvent))
					.TestCases(null, i => i.Site = siteId);
			}
		}

		public static IEnumerable OutTerminatedInOut
		{
			get
			{
				var personId = Guid.NewGuid();
				var siteId = Guid.NewGuid();
				var events = new IEvent[]
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonAssociationChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-18 12:04".Utc(),
						SiteId = null
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = siteId,
						Timestamp = "2015-02-18 12:08".Utc()
					},
				};

				return events
					.Permutations()
					.Where(p => !(p.First() is PersonAssociationChangedEvent))
					.TestCases(null, i => i.Site = siteId);
			}
		}

		public static IEnumerable OutSiteChangeInOut
		{
			get
			{
				var personId = Guid.NewGuid();
				var originSiteId = Guid.NewGuid();
				var destinationSiteid = Guid.NewGuid();

				var setup = new IEvent[]
				{
					new PersonInAdherenceEvent
					{
						PersonId = Guid.NewGuid(),
						SiteId = originSiteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonInAdherenceEvent
					{
						PersonId = Guid.NewGuid(),
						SiteId = destinationSiteid,
						Timestamp = "2015-02-18 12:02".Utc()
					},
				};

				var events = new IEvent[]
				{
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = originSiteId,
						Timestamp = "2015-02-18 12:02".Utc()
					},
					new PersonAssociationChangedEvent
					{
						PersonId = personId,
						Timestamp = "2015-02-18 12:04".Utc(),
						SiteId = destinationSiteid
					},
					new PersonInAdherenceEvent
					{
						PersonId = personId,
						SiteId = destinationSiteid,
						Timestamp = "2015-02-18 12:06".Utc()
					},
					new PersonOutOfAdherenceEvent
					{
						PersonId = personId,
						SiteId = destinationSiteid,
						Timestamp = "2015-02-18 12:08".Utc()
					}
				};

				return events
					.Permutations()
					.TestCases(setup, i =>
					{
						i.OriginSite = originSiteId;
						i.DestinationSite = destinationSiteid;
					});

			}
		}

	}
}