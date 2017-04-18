using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.FileImport;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.FileImport.ViewModels
{
    public class PreviewQueueGridViewModel : IModel
    {
        private TimeZoneInfo _timeZoneInfo;
        private ImportFileDoCollection _importAll;

        public PreviewQueueGridViewModel(string streamPath, string separator, TimeZoneInfo timeZoneInfo, Encoding encoding)
        {
            _importAll = new ImportFileDoCollection(streamPath, separator, encoding);
            _timeZoneInfo = timeZoneInfo;
        }

        public IList<ImportFileDo> ImportPreview
        {
            get
            {
                IList<ImportFileDo> ret = new List<ImportFileDo>();
                int upper = Math.Min(100, _importAll.Count);
                for (int i = 0; i < upper; i++)
                {
                    ret.Add(_importAll[i]);
                }
                return ret;
            }
        }

        public IModel NextStep()
        {
            Persister.Persist(_importAll, _timeZoneInfo);
            return null;
        }
    }
}