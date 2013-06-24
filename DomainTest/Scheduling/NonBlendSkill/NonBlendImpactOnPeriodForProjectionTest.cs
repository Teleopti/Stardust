using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.NonBlendSkill
{
	[TestFixture]
	public class NonBlendImpactOnPeriodForProjectionTest
	{
        private INonBlendSkillImpactOnPeriodForProjection _target;
		private IVisualLayerCollection _layerCollection1;
		private IVisualLayerCollection _layerCollection2;
		private IVisualLayerCollection _layerCollection3;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IPerson _person1;
		private IPerson _person2;
		private IVisualLayer _visualLayer1;
		private IVisualLayer _visualLayer2;
		private IActivity _mejeriVaror;
		private IActivity _lunch;
		private IAbsence _absence;
		private IVisualLayerFactory _visualLayerFactory;
		private ResourceCalculationDataContainer _resources;
		private DateTimePeriod _shiftPeriod;
		private ISkill _skillCarnaby;
		private ISkill _skillPhone;
		private IPersonSkillProvider _personSkillProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
            _target = new NonBlendSkillImpactOnPeriodForProjection();
			_lunch = ActivityFactory.CreateActivity("lunch");
			_mejeriVaror = ActivityFactory.CreateActivity("mejeri");
			_absence = AbsenceFactory.CreateAbsence("Vacation");
			_skillCarnaby = SkillFactory.CreateNonBlendSkill("Carnaby Street");
		    _skillCarnaby.Activity = _mejeriVaror;
			_skillCarnaby.SetId(Guid.NewGuid());
			_skillPhone = SkillFactory.CreateSkill("The Mall");
		    _skillPhone.Activity = _mejeriVaror;
			_skillPhone.SetId(Guid.NewGuid());
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillCarnaby, _skillPhone });
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skillCarnaby, _skillPhone });
			_visualLayerFactory = new VisualLayerFactory();
			_shiftPeriod = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc),
															 new DateTime(2010, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			_visualLayer1 = _visualLayerFactory.CreateShiftSetupLayer(_mejeriVaror, _shiftPeriod,_person1);
			_layerCollection1 = new VisualLayerCollection(_person1, new List<IVisualLayer> { _visualLayer1 }, new ProjectionPayloadMerger());
			_visualLayer2 = _visualLayerFactory.CreateShiftSetupLayer(_lunch, _shiftPeriod, _person1);
			_layerCollection2 = new VisualLayerCollection(_person2, new List<IVisualLayer> { _visualLayer2 }, new ProjectionPayloadMerger());

			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(_personSkillProvider);
			foreach (var layer in new []{_layerCollection1,_layerCollection2})
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(layer.Person, new DateOnly(2008, 1, 1), resourceLayer);
				}
			}

			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillCarnaby,
																			   new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc), 60,
																			   0, 0);
			ISkillDay skillDay = new SkillDay(new DateOnly(2010, 1, 1), _skillCarnaby,
											  ScenarioFactory.CreateScenarioAggregate(), new List<IWorkloadDay>(),
											  new List<ISkillDataPeriod>());
			_skillStaffPeriod.SetSkillDay(skillDay);
		}

		[Test]
		public void ShouldCalculateIfActivityIntersectsPeriod()
		{
            double result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillCarnaby);
			Assert.AreEqual(0.5, result);
		}

        [Test]
        public void ShouldCalculateOnOneLayerCollectionToo()
        {
            double result = _target.CalculatePeriod(_skillStaffPeriod, _layerCollection1, _mejeriVaror);
            Assert.AreEqual(0.5, result);
        }

		[Test]
		public void ShouldNotCalculateIfPayloadNotActivity()
		{
			var period = new DateTimePeriod(new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2010, 1, 1, 9, 30, 0, DateTimeKind.Utc));

			IVisualLayer baseactivityLayer = _visualLayerFactory.CreateShiftSetupLayer(_mejeriVaror, period, _person2);
			IVisualLayer absenceLayer = _visualLayerFactory.CreateAbsenceSetupLayer(_absence, baseactivityLayer, period);
			_layerCollection3 = new VisualLayerCollection(_person2, new List<IVisualLayer> {absenceLayer, _visualLayer1},
			                                              new ProjectionPayloadMerger());

			_resources.Clear();

			foreach (var resourceLayer in _layerCollection3.ToResourceLayers(15))
			{
				_resources.AddResources(_person2, new DateOnly(2008, 1, 1), resourceLayer);
			}

			double result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillCarnaby);
			Assert.AreEqual(0.5, result);
		}

		[Test]
		public void ShouldCalculateOnlyIfPersonSkillExist()
		{
            double result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillCarnaby);
			Assert.AreEqual(0.5, result);

			IPersonPeriod personPeriod = _person1.Period(new DateOnly(2010, 1, 1));
			IPersonSkill personSkill = personPeriod.PersonNonBlendSkillCollection[0];
			Assert.AreEqual(_skillCarnaby, personSkill.Skill);
            personPeriod.PersonNonBlendSkillCollection.Remove(personSkill);

            result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillCarnaby);
			Assert.AreEqual(0, result);
		}

		[Test]
		public void ShouldNotCalculateIfPersonPeriodIsNull()
		{
			_person2.RemoveAllPersonPeriods();
			_person1.RemoveAllPersonPeriods();
            double result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillCarnaby);
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
            double result = _target.CalculatePeriod(_skillStaffPeriod, _resources, _skillPhone);
			Assert.AreEqual(0, result);
		}

	}

    
}