using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting
{
    internal interface IPdfScheduleTemplate
    {
        float Height { get; }
        PdfTemplate Template { get; }
    }
}