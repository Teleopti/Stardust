using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class ForecastFile : AggregateRoot_Events_ChangeInfo_BusinessUnit, IForecastFile
    {
        private readonly byte[] _fileContent;
        private readonly string _fileName;

        protected ForecastFile(){}

        public ForecastFile(string fileName , byte[] fileContent)
        {
            _fileContent = fileContent;
            _fileName = fileName;
        }

        public virtual byte[] FileContent => _fileContent;

	    public virtual string FileName => _fileName;
    }
}
