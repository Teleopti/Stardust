using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class CombinedRestrictionTest
	{
		[Test]
		public static void MayNotMatchIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.False();
		}

		[Test]
		public static void MayNotMatchIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			two.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.False();
		}

		[Test]
		public static void MayMatchIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchWithShifts()).Return(true);
			two.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.True();
		}

		[Test]
		public static void MayNotMatchIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			target.MayMatchWithShifts().Should().Be.False();
		}





		[Test]
		public static void MayMatchBlacklistedShiftsIfFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public static void MayMatchBlacklistedShiftsIfSecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			two.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public static void MayMatchBlacklistedShiftsIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);
			two.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public static void MayNotMatchBlacklistedShiftsIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			target.MayMatchBlacklistedShifts().Should().Be.False();
		}





		[Test]
		public static void ShouldNotMatchShiftCategoryIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			one.Stub(x => x.Match(shiftCategory)).Return(true);

			target.Match(shiftCategory).Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchShiftCategoryIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			two.Stub(x => x.Match(shiftCategory)).Return(true);

			target.Match(shiftCategory).Should().Be.False();
		}

		[Test]
		public static void ShouldMatchShiftCategoryIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			one.Stub(x => x.Match(shiftCategory)).Return(true);
			two.Stub(x => x.Match(shiftCategory)).Return(true);

			target.Match(shiftCategory).Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchShiftCategoryIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			target.Match(shiftCategory).Should().Be.False();
		}






		[Test]
		public static void ShouldNotMatchIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			one.Stub(x => x.Match(workShiftProjection)).Return(true);

			target.Match(workShiftProjection).Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			two.Stub(x => x.Match(workShiftProjection)).Return(true);

			target.Match(workShiftProjection).Should().Be.False();
		}

		[Test]
		public static void ShouldMatchIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			one.Stub(x => x.Match(workShiftProjection)).Return(true);
			two.Stub(x => x.Match(workShiftProjection)).Return(true);

			target.Match(workShiftProjection).Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			target.Match(workShiftProjection).Should().Be.False();
		}


	}
}