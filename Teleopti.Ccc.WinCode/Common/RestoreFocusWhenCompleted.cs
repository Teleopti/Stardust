using System;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Sets the focus to the target when disposed
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-06-17
    /// </remarks>
    public class RestoreFocusWhenCompleted : IDisposable
    {
        private readonly IFocusTarget _target;

        public RestoreFocusWhenCompleted(IFocusTarget target)
        {
            _target = target;
        }

        public void Dispose()
        {
          
           Dispose(true);
           GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_target != null)
                {
                    _target.SetFocus();
                  
                }
            }
        }

    }

    
}
