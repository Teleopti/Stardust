using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Infrastructure
{
    [TestFixture]
    public class ComplexRuleSetEventArgsTest
    {
        [Test]
        public void ShouldCreateMessageArgs()
        {
            const string ruleSetName = "My ruleset";
            var target = new ComplexRuleSetEventArgs(ruleSetName);
            target.RuleSetName.Should().Be.EqualTo(ruleSetName);
        }
    }

    [TestFixture]
    public class DenormalizeScheduleProjectionTest
    {
        [Test]
        public void ShouldCreateDenormalizeMessage()
        {
#pragma warning disable 612,618
            var target = new DenormalizeScheduleProjection();
#pragma warning restore 612,618
            target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
        }
    }

    [TestFixture]
    public class ScheduleChangedTest
    {
        [Test]
        public void ShouldCreateScheduleChangedMessage()
        {
#pragma warning disable 612,618
            var target = new ScheduleChanged();
#pragma warning restore 612,618
            target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
        }
    }
}