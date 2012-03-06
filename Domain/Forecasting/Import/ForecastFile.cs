using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class ForecastFile : AggregateRootWithBusinessUnit, IForecastFile
    {

        private byte[] _fileContent;
        private string _fileName;

        protected ForecastFile()
        {
        }

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
