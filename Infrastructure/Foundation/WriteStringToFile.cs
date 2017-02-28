using System.IO;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class WriteStringToFile : IWriteToFile
    {
        public void Save(string fileName, string content)
        {
            File.WriteAllText(fileName, content);
        }
    }
}
