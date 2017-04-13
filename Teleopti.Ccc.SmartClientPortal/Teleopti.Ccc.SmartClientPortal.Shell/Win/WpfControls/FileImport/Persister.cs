using System;
using System.Data;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.FileImport;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.FileImport
{
    public static class Persister
    {
        public static void Persist(ImportFileDoCollection collection, TimeZoneInfo timeZoneInfo)
        {
            DimQueueTable dimQueTable = new DimQueueTable(timeZoneInfo);
            DataTable dt = dimQueTable.CreateEmptyDataTable();

            dimQueTable.Fill(dt, collection);

            IStatisticRepository statisticRepository = StatisticRepositoryFactory.Create();

            statisticRepository.DeleteStgQueues();
            statisticRepository.PersistFactQueues(dt);
            statisticRepository.LoadDimQueues();
            statisticRepository.LoadFactQueues();
        }
    }
}