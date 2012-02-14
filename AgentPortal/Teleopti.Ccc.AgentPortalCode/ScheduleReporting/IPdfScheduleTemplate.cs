using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public interface IPdfScheduleTemplate
    {
        float Height { get; }
        PdfTemplate Template { get; }
    }
}