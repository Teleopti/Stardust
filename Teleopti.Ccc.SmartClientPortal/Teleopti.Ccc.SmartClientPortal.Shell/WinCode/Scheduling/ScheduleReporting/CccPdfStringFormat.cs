using Syncfusion.Pdf.Graphics;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
    public class CccPdfStringFormat
    {
        public PdfWordWrapType WordWrapType { get; set; }
        public PdfTextAlignment Alignment { get; set; }
        public PdfVerticalAlignment LineAlignment { get; set; }

        public CccPdfStringFormat()
        {
            WordWrapType = PdfWordWrapType.Word;
            Alignment = PdfTextAlignment.Left;
            LineAlignment = PdfVerticalAlignment.Top;
        }

        //Always set RTL to true, bug in syncfusion pdfgrapthics.drawstring. Text is mirrored when rtl = false and text is arabic
        public PdfStringFormat PdfStringFormat
        {
            get { return new PdfStringFormat {RightToLeft = true, WordWrap = WordWrapType, Alignment = Alignment, LineAlignment = LineAlignment}; }   
        }
    }
}
