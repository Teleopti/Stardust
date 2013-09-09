using System;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	/// <summary>
	/// Tests for Main shift
	/// </summary>
	[TestFixture]
	public class MainShiftTest //: ShiftTest<MainShift>
	{
		private ShiftCategory shiftCat;
		private IMainShift target;

		[SetUp]
		public void Setup()
		{
			shiftCat = ShiftCategoryFactory.CreateShiftCategory("hej");
			target = new MainShift(shiftCat);
		}


		/// <summary>
		/// Verifies the properties defaults ok.
		/// </summary>
		[Test]
		public void VerifyPropertiesDefaultsOk()
		{
			Assert.AreEqual(shiftCat.Description.Name, target.ShiftCategory.Description.Name);
			Assert.AreEqual(0, target.LayerCollection.Count);
		}

		/// <summary>
		/// Verifies that a new shiftCategory can be set
		/// </summary>
		[Test]
		public void CanSetShiftCategory()
		{
			ShiftCategory newCat = new ShiftCategory("Kväll");
			target.ShiftCategory = newCat;
			Assert.AreSame(newCat, target.ShiftCategory);
		}

		/// <summary>
		/// Verifies the clone method.
		/// </summary>
		[Test]
		public void VerifyClone()
		{
			DateTimePeriod period1 =
				 new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			//ActivityLayer actLay1 = new ActivityLayer(DummyActivity, period1);
			DateTimePeriod period2 =
				 new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			//ActivityLayer actLay2 = new ActivityLayer(DummyActivity, period2);
			typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,
																																  Guid.NewGuid());
			target.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period1));
			target.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period2));
			MainShift clone = (MainShift)target.Clone();

			Assert.AreNotSame(target, clone);
			Assert.AreNotSame(target.LayerCollection, clone.LayerCollection);
			Assert.AreSame(target.LayerCollection[0].Payload, clone.LayerCollection[0].Payload);
			Assert.AreSame(target.LayerCollection[1].Payload, clone.LayerCollection[1].Payload);
			Assert.AreEqual(target.LayerCollection[0].Period, clone.LayerCollection[0].Period);
			Assert.AreEqual(target.LayerCollection[1].Period, clone.LayerCollection[1].Period);
			//Assert.IsNull(clone.Id);
			Assert.AreEqual(target.Id, clone.Id);
			Assert.AreSame(target.ShiftCategory, clone.ShiftCategory);
		}

		[Test]
		public void VerifyICloneableEntity()
		{
			target.ShiftCategory.SetId(Guid.NewGuid());
			DateTimePeriod period1 =
				 new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			//ActivityLayer actLay1 = new ActivityLayer(DummyActivity, period1);
			DateTimePeriod period2 =
				 new DateTimePeriod(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			//ActivityLayer actLay2 = new ActivityLayer(DummyActivity, period2);
			typeof(Entity).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,
																																  Guid.NewGuid());
			MainShiftActivityLayer layer;
			layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period1);
			((IEntity)layer).SetId(Guid.NewGuid());
			target.LayerCollection.Add(layer);

			layer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("hej"), period2);
			((IEntity)layer).SetId(Guid.NewGuid());
			target.LayerCollection.Add(layer);

			MainShift mainShift = (MainShift)target.EntityClone();
			Assert.AreEqual(target.Id, mainShift.Id);
			Assert.AreEqual(target.ShiftCategory.Id, mainShift.ShiftCategory.Id);
			Assert.AreEqual(((IMainShiftActivityLayer)target.LayerCollection[0]).Id, ((IMainShiftActivityLayer)mainShift.LayerCollection[0]).Id);

			mainShift = (MainShift)target.NoneEntityClone();
			Assert.AreEqual(target.ShiftCategory.Id, mainShift.ShiftCategory.Id);
			Assert.IsNull(mainShift.Id);
			Assert.IsNull(((IMainShiftActivityLayer)mainShift.LayerCollection[0]).Id);
		}

		/// <summary>
		/// Protected constructor works.
		/// </summary>
		[Test]
		public void ProtectedConstructorWorks()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyShiftCategoryIsNotNullInConstructor()
		{
			target = new MainShift(null);
		}

		[Test]
		public void CloneShouldContainReferenceToShiftCategory()
		{
			IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("hej");
			IMainShift mainShift = new MainShift(category);
			target = (IMainShift)mainShift.Clone();
			Assert.AreSame(category, target.ShiftCategory);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyShiftCategoryIsNotNullInSetter()
		{
			target.ShiftCategory = null;
		}

		private class testMainShift : MainShift
		{
			internal testMainShift()
				: base()
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CannotAddNothingButMainShiftActivityLayer()
		{
			target.LayerCollection.Add(new ActivityLayer(new Activity("fd"), new DateTimePeriod(2002, 1, 1, 2003, 1, 1)));
		}
		
		[Test]
		public void ShouldNotEqualAnAssignmentWithSameId()
		{
			var ass = new PersonAssignment(new Person(), new Scenario("d"));
			var ms = new MainShift(shiftCat);
			var id = Guid.NewGuid();
			ass.SetId(id);
			ms.SetId(id);
			ms.Should().Not.Be.EqualTo(ass);
			ass.Should().Not.Be.EqualTo(ms);
		}
	}
}