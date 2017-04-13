using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Teleopti.Ccc.WinCode.FileImport
{
    public class ImportFileDoCollection : Collection<ImportFileDo>
    {
        public ImportFileDoCollection(string streamPath, string separator, Encoding encoding)
        {
            var reader = new StreamReader(streamPath, encoding, false);
            
                var lineNo = 0;
                while (!reader.EndOfStream)
                {
                    lineNo++;
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        {
                            Add(ImportFileDo.Create(line, separator, lineNo));
                        }
                }
        }
    }
}