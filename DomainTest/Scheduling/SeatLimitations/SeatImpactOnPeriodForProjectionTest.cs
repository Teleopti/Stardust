using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class SeatImpactOnPeriodForProjectionTest
	{
		private ISeatImpactOnPeriodForProjection _target;
		private IVisualLayerCollection _layerCollection1;
		private IVisualLayerCollection _layerCollection2;
		private IVisualLayerCollection _layerCollection3;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IPerson _person1;
		private IPerson _person2;
		private IVisualLayer _visualLayer1;
		private IVisualLayer _visualLayer2;
		private IActivity _phone;
		private IActivity _lunch;
		private IAbsence _absence;
		private IVisualLayerFactory _visualLayerFactory;
		private IList<IVisualLayerCollection> _shiftList;
		private DateTimePeriod _shiftPeriod;
		private ISkill _skillLondon;
		private ISkill _skillPhone;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_target = new SeatImpactOnPeriodForProjection();
			_lunch = ActivityFactory.CreateActivity("lunch");
			_phone = ActivityFactory.CreateActivity("phone");
			_absence = AbsenceFactory.CreateAbsence("vacation");
			_phone.RequiresSeat = true;
			_skillLondon = SkillFactory.CreateSiteSkill("London");
			_skillPhone = SkillFactory.CreateSkill("Phone");
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillLondon, _skillPhone });
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillLondon, _skillPhone });
			_visualLayerFactory = new VisualLayerFactory();
			_shiftPeriod = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc),
															 new DateTime(2010, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			_visualLayer1	 = _visualLayerFactory.CreateShiftSetupLayer(_phone, _shiftPeriod,_person1);
			_layerCollection1 = new VisualLayerCollection(_person1, new List<IVisualLayer> { _visualLayer1 }, new ProjectionPayloadMerger());
			_visualLayer2 = _visualLayerFactory.CreateShiftSetupLayer(_lunch, _shiftPeriod,_person2);
			_layerCollection2 = new VisualLayerCollection(_person2, new List<IVisualLayer> { _visualLayer2 }, new ProjectionPayloadMerger());



			_shiftList = new List<IVisualLayerCollection> { _layerCollection1, _layerCollection2, };


			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillLondon,
																			   new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc), 60,
																			   0, 0);
			ISkillDay skillDay = new SkillDay(new DateOnly(2010, 1, 1), _skillLondon,
											  ScenarioFactory.CreateScenarioAggregate(), new List<IWorkloadDay>(),
											  new List<ISkillDataPeriod>());
			_skillStaffPeriod.SetSkillDay(skillDay);
		}

		[Test]
		public void ShouldCalculateIfActivityIntersectsPeriod()
		{

			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldOnlyCalculateIfActivityRequiresSeat()
		{
			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0.5, result);
			_lunch.RequiresSeat = true;
			result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void ShouldCalculateUnderlyingPayload()
		{
			IMeeting meeting = new Meeting(_person1, new List<IMeetingPerson>(), "subject","location","description",_phone, new Scenario("scenario"));
			IPayload payload = new MeetingPayload(meeting);
			IVisualLayer visualLayer = new VisualLayer(payload, _shiftPeriod, _phone, _person1);
			IVisualLayerCollection layerCollection = new VisualLayerCollection(_person1, new List<IVisualLayer> { visualLayer }, new ProjectionPayloadMerger());
			IList<IVisualLayerCollection> shiftList = new List<IVisualLayerCollection> {layerCollection};

			var result = _target.CalculatePeriod(_skillStaffPeriod, shiftList);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldNotCalculateIfPayloadNotActivity()
		{
			var period = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc),
				new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc));

			var baseactivityLayer = _visualLayerFactory.CreateShiftSetupLayer(_phone, period, _person2);
			var absenceLayer = _visualLayerFactory.CreateAbsenceSetupLayer(_absence, baseactivityLayer, period,
				baseactivityLayer.PersonAbsenceId);
			_layerCollection3 = new VisualLayerCollection(_person2, new List<IVisualLayer> {absenceLayer, _visualLayer1},
				new ProjectionPayloadMerger());
			_shiftList = new List<IVisualLayerCollection> {_layerCollection3 };

			var result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldCalculateOnlyIfPersonSkillExist()
		{
			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0.5, result);


			IPersonPeriod personPeriod = _person1.Period(new DateOnly(2010, 1, 1));
			IPersonSkill personSkill = personPeriod.PersonMaxSeatSkillCollection[0];
			Assert.AreEqual(_skillLondon, personSkill.Skill);
			personPeriod.PersonMaxSeatSkillCollection.Remove(personSkill);


			result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void ShouldNotCalculateIfPersonPeriodIsNull()
		{
			_person2.RemoveAllPersonPeriods();
			_person1.RemoveAllPersonPeriods();
			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void ShouldNotCalculateIfSkillStaffPeriodSkillIsWrongType()
		{
			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillPhone,
																			   new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc), 60,
																			   0, 0);
			ISkillDay skillDay = new SkillDay(new DateOnly(2010, 1, 1), _skillPhone,
											  ScenarioFactory.CreateScenarioAggregate(), new List<IWorkloadDay>(),
											  new List<ISkillDataPeriod>());
			_skillStaffPeriod.SetSkillDay(skillDay);
			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0, result);
		}

	}
}