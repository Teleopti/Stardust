using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    public class ImportForecastRepositoryTest : RepositoryTest<IForecastFile>
    {
        protected override IForecastFile CreateAggregateWithCorrectBusinessUnit()
        {
            const string fileName = "TestImportFile.csv";
            const string fileContent = "Test Forecast Import";
            var encoding = new UTF8Encoding();
            var forecastFile = new ForecastFile(fileName,encoding.GetBytes(fileContent));
            return forecastFile;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void VerifyAggregateGraphProperties(IForecastFile loadedAggregateFromDatabase)
        {
            IForecastFile forecastFile = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(forecastFile.FileName, loadedAggregateFromDatabase.FileName);
            Assert.AreEqual(forecastFile.FileContent, loadedAggregateFromDatabase.FileContent);
        }

        protected override Repository<IForecastFile> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ImportForecastsRepository.DONT_USE_CTOR(currentUnitOfWork);
        }
    }
}
