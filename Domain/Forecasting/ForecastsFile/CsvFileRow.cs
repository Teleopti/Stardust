using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IFileRow : IList<string>
    {
        string LineText { get; set; }
    }
    
    [Serializable]
    public class CsvFileRow : List<string>, IFileRow, ICloneable
    {
        public string LineText { get; set; }
        
        public object Clone()
        {
            return this.Select(item => item.Clone()).ToList();
        }
    }
}