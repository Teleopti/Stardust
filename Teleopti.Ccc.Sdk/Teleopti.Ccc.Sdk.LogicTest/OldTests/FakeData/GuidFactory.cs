using System;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests.FakeData
{
    public static class GuidFactory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Guid GetGuid()
        {
            return new Guid("5f4e40e1-2b69-4184-b0c5-4cfe27a7d85f");
        }
    }
}