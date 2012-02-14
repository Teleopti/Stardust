using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public interface IPdfScheduleTemplate
    {
        float Height { get; }
        PdfTemplate Template { get; }
    }
}