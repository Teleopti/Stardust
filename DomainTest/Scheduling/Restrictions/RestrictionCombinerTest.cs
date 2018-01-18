using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class RestrictionCombinerTest
	{
		[Test]
		public void ShouldReturnNullWhenRestrictionsIsNull()
		{
			var target = new RestrictionCombiner();

			var result = target.CombineEffectiveRestrictions(null, new EffectiveRestriction());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenEffectiveRestrictionIsNull()
		{
			var target = new RestrictionCombiner();

			var result = target.CombineEffectiveRestrictions(new IEffectiveRestriction[]{new EffectiveRestriction()}, null);

			result.Should().Be.Null();
		}
	}
}
