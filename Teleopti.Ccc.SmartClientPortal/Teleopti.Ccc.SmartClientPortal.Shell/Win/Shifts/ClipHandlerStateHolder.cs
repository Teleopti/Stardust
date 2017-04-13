namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public class ClipHandlerStateHolder
    {
        private object value;

        private static ClipHandlerStateHolder _instance;

        public static ClipHandlerStateHolder Current
        {
            get
            {
                if (_instance == null)
                    _instance = new ClipHandlerStateHolder();
                return _instance;
            }
        }

        public void Set(object newValue)
        {
            this.value = newValue;
        }

        public object Get()
        {
            return this.value;
        }
    }
}