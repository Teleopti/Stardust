using System;
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
		public void MayNotMatchIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.False();
		}

		[Test]
		public void MayNotMatchIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			two.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.False();
		}

		[Test]
		public void MayMatchIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchWithShifts()).Return(true);
			two.Stub(x => x.MayMatchWithShifts()).Return(true);

			target.MayMatchWithShifts().Should().Be.True();
		}

		[Test]
		public void MayNotMatchIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			target.MayMatchWithShifts().Should().Be.False();
		}





		[Test]
		public void MayMatchBlacklistedShiftsIfFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public void MayMatchBlacklistedShiftsIfSecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			two.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public void MayMatchBlacklistedShiftsIfBothMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			one.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);
			two.Stub(x => x.MayMatchBlacklistedShifts()).Return(true);

			target.MayMatchBlacklistedShifts().Should().Be.True();
		}

		[Test]
		public void MayNotMatchBlacklistedShiftsIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);

			target.MayMatchBlacklistedShifts().Should().Be.False();
		}





		[Test]
		public void ShouldNotMatchShiftCategoryIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			one.Stub(x => x.Match(shiftCategory)).Return(true);

			target.Match(shiftCategory).Should().Be.False();
		}

		[Test]
		public void ShouldNotMatchShiftCategoryIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			two.Stub(x => x.Match(shiftCategory)).Return(true);

			target.Match(shiftCategory).Should().Be.False();
		}

		[Test]
		public void ShouldMatchShiftCategoryIfBothMatch()
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
		public void ShouldNotMatchShiftCategoryIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var shiftCategory = new ShiftCategory(" ");

			target.Match(shiftCategory).Should().Be.False();
		}






		[Test]
		public void ShouldNotMatchIfOnlyFirstMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			one.Stub(x => x.Match(workShiftProjection)).Return(true);

			target.Match(workShiftProjection).Should().Be.False();
		}

		[Test]
		public void ShouldNotMatchIfOnlySecondMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			two.Stub(x => x.Match(workShiftProjection)).Return(true);

			target.Match(workShiftProjection).Should().Be.False();
		}

		[Test]
		public void ShouldMatchIfBothMatch()
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
		public void ShouldNotMatchIfNoneMatch()
		{
			var one = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var two = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var target = new CombinedRestriction(one, two);
			var workShiftProjection = new WorkShiftProjection();

			target.Match(workShiftProjection).Should().Be.False();
		}


	}
}