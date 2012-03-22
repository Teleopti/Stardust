using System.Collections.Generic;
using System.Text;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IFileRow
    {
        IList<string> Content { get; }
        int Count { get; }
        void Clear();
    }
    
    public class CsvFileRow : IFileRow
    {
        private readonly IList<string> _content;
        private const char separator = ',';

        public CsvFileRow()
        {
            _content = new List<string>();
        }

        public CsvFileRow(string lineText)
        {
            _content = new List<string>(lineText.Split(separator));
        }

        public IList<string> Content { get { return _content; } }

        public void Clear()
        {
            _content.Clear();
        }

        public int Count { get { return _content.Count; } }

        public override string ToString()
        {
            var line = new StringBuilder();
            for (var i = 0; i < _content.Count-1; i++ )
            {
                line.Append(_content[i] + separator);
            }
            line.Append(_content[Count - 1]);
            return line.ToString();
        }
    }
}