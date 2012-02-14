using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    [TestFixture]
    public class SkillVisualLayerCollectionDictionaryCreatorTest
    {
        private ISkillVisualLayerCollectionDictionaryCreator _target;
        private DateOnly _day;
        private IPerson _person1;
        private IPerson _person2;
        private ISite _siteWithMaxSeats;
        private ISite _siteNoMaxSeats;
        private ITeam _team1;
		private ITeam _team2;
        private ISkill _siteSkill;
    	private IVisualLayerCollection _layerCollection1;
		private IVisualLayerCollection _layerCollection2;
		private IVisualLayerFactory _visualLayerFactory;
		private IVisualLayer _visualLayer1;
		private IVisualLayer _visualLayer2;
		private IActivity _phone;
		private DateTimePeriod _shiftPeriod;
    	private IList<IVisualLayerCollection> _layerCollectionList;

        [SetUp]
        public void Setup()
        {
			_visualLayerFactory = new VisualLayerFactory();
            _day = new DateOnly(2010, 10, 10);
			_phone = new Activity("phone");
            _person1 = PersonFactory.CreatePerson("Person1");
            _person2 = PersonFactory.CreatePerson("Person2");
            _siteWithMaxSeats = SiteFactory.CreateSimpleSite("Site1");
            _siteNoMaxSeats = SiteFactory.CreateSimpleSite("Site2");
            _team1 = TeamFactory.CreateSimpleTeam("Team1");
			_team2 = TeamFactory.CreateSimpleTeam("Team2");
            _siteSkill = SkillFactory.CreateSiteSkill("SiteSkill");
			_shiftPeriod = new DateTimePeriod(new DateTime(2010, 10, 10, 9, 30, 0, DateTimeKind.Utc),
															 new DateTime(2010, 10, 10, 11, 0, 0, DateTimeKind.Utc));
			_visualLayer1 = _visualLayerFactory.CreateShiftSetupLayer(_phone, _shiftPeriod);
			_layerCollection1 = new VisualLayerCollection(_person1, new List<IVisualLayer> { _visualLayer1 }, new ProjectionPayloadMerger());
			_visualLayer2 = _visualLayerFactory.CreateShiftSetupLayer(_phone, _shiftPeriod);
			_layerCollection2 = new VisualLayerCollection(_person2, new List<IVisualLayer> { _visualLayer2 }, new ProjectionPayloadMerger());
			_layerCollectionList = new List<IVisualLayerCollection> { _layerCollection1, _layerCollection2 };
			_target = new SkillVisualLayerCollectionDictionaryCreator();

        }

		[Test]
		public void ShouldSortOutSitesWithMaxSeats()
		{
			_siteWithMaxSeats.MaxSeatSkill = _siteSkill;
			_siteWithMaxSeats.AddTeam(_team1);
			_siteNoMaxSeats.AddTeam(_team2);

			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 01, 01), _team1);
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 01, 01), _team2);

			_person1.AddPersonPeriod(personPeriod1);
			_person2.AddPersonPeriod(personPeriod2);

			SkillVisualLayerCollectionDictionary result = _target.CreateSiteVisualLayerCollectionDictionary(_layerCollectionList, _day);
			Assert.AreEqual(1, result.Count);

			IList<IVisualLayerCollection> collection = result[_siteWithMaxSeats.MaxSeatSkill];
			Assert.AreEqual(1, collection.Count);
		}

		[Test]
		public void ShouldNotHandleCollectionsWhenPersonPeriodIsNull()
		{
			_siteWithMaxSeats.MaxSeatSkill = _siteSkill;
			_siteWithMaxSeats.AddTeam(_team1);
			_siteNoMaxSeats.AddTeam(_team2);

			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2100, 01, 01), _team1); //In the future
			IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2100, 01, 01), _team2); //In the future

			_person1.AddPersonPeriod(personPeriod1);
			_person2.AddPersonPeriod(personPeriod2);

			SkillVisualLayerCollectionDictionary result = _target.CreateSiteVisualLayerCollectionDictionary(_layerCollectionList, _day);
			Assert.AreEqual(0, result.Count);
		}
    }
}
