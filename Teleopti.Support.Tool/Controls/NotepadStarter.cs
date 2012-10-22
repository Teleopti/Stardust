using System;
using System.Diagnostics;
using System.Threading;

namespace Teleopti.Support.Tool.Controls
{
    public class NotepadStarter : IDisposable
    {
        private readonly string _fileName;
        private Process _process;
        private ThreadInputAttachment _threadInputAttachment;

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

            int threadId = _process.Threads[0].Id;
            _threadInputAttachment = new ThreadInputAttachment(threadId);

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
            if (_threadInputAttachment != null)
            {
                _threadInputAttachment.Dispose();
            }
            if (_process != null)
            {
                _process.Dispose();
            }
        }
    }
}