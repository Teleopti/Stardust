using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    public static class TestState
    {
		public static TestDataFactory TestDataFactory;
		public static DatabaseHelper.Backup Ccc7DataBackup;
	    public static IUnitOfWork UnitOfWork;

	    public static IBusinessUnit BusinessUnit;
    }
}