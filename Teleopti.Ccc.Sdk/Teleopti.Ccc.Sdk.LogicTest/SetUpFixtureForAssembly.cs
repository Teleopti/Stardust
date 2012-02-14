

using NUnit.Framework;
using Teleopti.Ccc.Sdk.LogicTest.OldTests;

namespace Teleopti.Ccc.Sdk.LogicTest
{
    /// <summary>
    /// Setup fixture for assembly
    /// </summary>
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), SetUp]
        public void RunBeforeAnyTest()
        {
            LogOn.RunAsPeterWestlinJunior();
        }
    }
}