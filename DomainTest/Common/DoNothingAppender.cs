using log4net.Appender;
using log4net.Core;

namespace Teleopti.Ccc.DomainTest.Common
{
    public class DoNothingAppender : IAppender
    {
        public void Close()
        {
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
        }

        public string Name
        {
            get { return "Appender that does nothing, used in tests"; }
            set { }
        }
    }
}
