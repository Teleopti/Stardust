using System;
using System.Globalization;
using System.Threading;

namespace Teleopti.Ccc.Domain.Common
{
    public sealed class UICultureContext : IDisposable
    {
        private readonly CultureInfo _previousCulture;

        public UICultureContext(CultureInfo cultureInfo)
        {
            _previousCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentUICulture = _previousCulture;
        }
    }
}