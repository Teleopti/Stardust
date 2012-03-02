using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IFileRow : IList<string>
    {
        string LineText { get; set; }
    }

    public class CsvFileRow : List<string>, IFileRow
    {
        public string LineText { get; set; }
    }
}