using Teleopti.Ccc.WinCode.Common.Models;

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
                NotifyProperty(()=>TestProperty);
            }
        }
    }

    public class DataModelWithInvalidStateForTest :DataModelWithInvalidState
    {
        private readonly string _alwaysErrorProperty;

        public DataModelWithInvalidStateForTest(string alwaysErrorProperty)
        {
            _alwaysErrorProperty = alwaysErrorProperty;
        }

        protected override string CheckProperty(string propertyName)
        {
            if (propertyName.Equals(_alwaysErrorProperty)) return "error";
            return null;
        }
    }
}
