using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

		[SetUp]
		public void Setup()
		{
			_target = new SeatImpactOnPeriodForProjection();
			_lunch = new Activity("lunch");
			_phone = new Activity("phone");
			_absence = new Absence();
			_phone.RequiresSeat = true;
			_skillLondon = SkillFactory.CreateSiteSkill("London");
			_skillPhone = SkillFactory.CreateSkill("Phone");
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillLondon, _skillPhone });
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillLondon, _skillPhone });
			_visualLayerFactory = new VisualLayerFactory();
			_shiftPeriod = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc),
															 new DateTime(2010, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			_visualLayer1 = _visualLayerFactory.CreateShiftSetupLayer(_phone, _shiftPeriod);
			_layerCollection1 = new VisualLayerCollection(_person1, new List<IVisualLayer> { _visualLayer1 }, new ProjectionPayloadMerger());
			_visualLayer2 = _visualLayerFactory.CreateShiftSetupLayer(_lunch, _shiftPeriod);
			_layerCollection2 = new VisualLayerCollection(_person2, new List<IVisualLayer> { _visualLayer2 }, new ProjectionPayloadMerger());



			_shiftList = new List<IVisualLayerCollection> { _layerCollection1, _layerCollection2, };


			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillLondon,
																			   new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc), 60,
																			   0, 0);
			ISkillDay skillDay = new SkillDay(new DateOnly(2010, 1, 1), _skillLondon,
											  ScenarioFactory.CreateScenarioAggregate(), new List<IWorkloadDay>(),
											  new List<ISkillDataPeriod>());
			_skillStaffPeriod.SetParent(skillDay);
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
		public void ShouldNotCalculateIfPayloadNotActivity()
		{
			IVisualLayer absenceLayer;
			IVisualLayer baseactivityLayer;

			DateTimePeriod period = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc),
															 new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc));

			baseactivityLayer = _visualLayerFactory.CreateShiftSetupLayer(_phone, period);
			absenceLayer = _visualLayerFactory.CreateAbsenceSetupLayer(_absence, baseactivityLayer, period);
			_layerCollection3 = new VisualLayerCollection(_person2, new List<IVisualLayer> { absenceLayer, _visualLayer1 }, new ProjectionPayloadMerger());
			_shiftList = new List<IVisualLayerCollection> {_layerCollection3 };

			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
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
			_skillStaffPeriod.SetParent(skillDay);
			double result = _target.CalculatePeriod(_skillStaffPeriod, _shiftList);
			Assert.AreEqual(0, result);
		}

	}
}