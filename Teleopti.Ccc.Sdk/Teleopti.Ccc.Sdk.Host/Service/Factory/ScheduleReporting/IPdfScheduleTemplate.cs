using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal interface IPdfScheduleTemplate
    {
        float Height { get; }
        PdfTemplate Template { get; }
    }
}