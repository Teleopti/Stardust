using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class ForecastFile : AggregateRootWithBusinessUnit, IForecastFile
    {
        private readonly byte[] _fileContent;
        private readonly string _fileName;

        protected ForecastFile(){}

        public ForecastFile(string filename , byte[] fileContent)
        {
            _fileContent = fileContent;
            _fileName = filename;
        }

        public virtual byte[] FileContent
        {
            get { return _fileContent; }
        }

        public virtual string FileName
        {
            get { return _fileName; }
        }
    }
}
