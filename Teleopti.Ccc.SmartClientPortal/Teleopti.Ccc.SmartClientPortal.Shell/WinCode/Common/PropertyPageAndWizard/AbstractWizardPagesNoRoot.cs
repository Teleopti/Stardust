namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Abstract base class for wizard managers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public abstract class AbstractWizardPagesNoRoot<T> : AbstractPropertyPagesNoRoot<T>
    {
        protected AbstractWizardPagesNoRoot(T stateObj) : base(stateObj)
        {
            StateObj = stateObj;
        }

        public override bool ModeCreateNew
        {
            get { return true; }
        }
    }
}
