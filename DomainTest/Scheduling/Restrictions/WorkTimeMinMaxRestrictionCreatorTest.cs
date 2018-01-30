using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"), TestFixture]
	public class WorkTimeMinMaxRestrictionCreatorTest
	{
		[Test]
		public static void ShouldReturnEffectiveRestriction()
		{
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var scheduleDay = new StubFactory().ScheduleDayStub();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var target = new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator);

			scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(new IPersonMeeting[0]);
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);

			var result = target.MakeWorkTimeMinMaxRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll());

			result.Restriction.Should().Be(effectiveRestriction);
		}
	}
}