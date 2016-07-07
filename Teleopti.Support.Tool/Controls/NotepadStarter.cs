using System;
using System.Diagnostics;

namespace Teleopti.Support.Tool.Controls
{
    public class NotepadStarter : IDisposable
    {
        private readonly string _fileName;
        private Process _process;

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
        public delegate void NotepadExitedDelegate(object sender, EventArgs e);
        public event NotepadExitedDelegate NotepadExited;

        public NotepadStarter(string fileName)
        {
            _fileName = fileName;
        }

        public void Run()
        {
            _process = new Process();
            _process.StartInfo.FileName = "Notepad.exe";
            _process.StartInfo.Arguments = _fileName;
            _process.StartInfo.UseShellExecute = false;
            _process.Exited += new EventHandler(_process_Exited);

            _process.Start();

            //while (!_process.HasExited)
            //{
            //    NotepadExited();
            //}
            //
        }

        void _process_Exited(object sender, EventArgs e)
        {
            NotepadExited(this, e);
        }

        public void Dispose()
        {
            if (_process != null)
            {
                _process.Dispose();
            }
        }
    }
}