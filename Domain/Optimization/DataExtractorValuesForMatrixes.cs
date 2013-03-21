using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IDataExtractorValuesForMatrixes
    {
        IDictionary<IScheduleMatrixPro, IScheduleResultDataExtractor> Data { get; }
        void Add(IScheduleMatrixPro scheduleMatrixPro, IScheduleResultDataExtractor extractor);
    }

    public class DataExtractorValuesForMatrixes : IDataExtractorValuesForMatrixes
    {
        public IDictionary<IScheduleMatrixPro, IScheduleResultDataExtractor> Data { get; private set; }

        public DataExtractorValuesForMatrixes()
        {
            Data = new Dictionary<IScheduleMatrixPro, IScheduleResultDataExtractor>();
        }

        public void Add(IScheduleMatrixPro scheduleMatrixPro, IScheduleResultDataExtractor extractor)
        {
            Data.Add(scheduleMatrixPro, extractor);
        }
    }
}