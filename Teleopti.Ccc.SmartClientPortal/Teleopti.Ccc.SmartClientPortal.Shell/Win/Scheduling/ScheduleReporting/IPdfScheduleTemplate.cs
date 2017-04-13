using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public interface IPdfScheduleTemplate
    {
        float Height { get; }
        PdfTemplate Template { get; }
    }
}