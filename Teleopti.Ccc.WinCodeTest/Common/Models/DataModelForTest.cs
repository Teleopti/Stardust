using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Models;

namespace Teleopti.Ccc.WinCodeTest.Common.Models
{
    public class DataModelForTest : DataModel
    {
        private string _testProperty;

        public string Test
        {
            get; set;
        }

        public string TestProperty
        {
            get { return _testProperty; }
            set
            {
                _testProperty = value;
                SendPropertyChanged(nameof(TestProperty));
            }
        }
    }
}
