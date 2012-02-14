﻿using Syncfusion.Pdf.Graphics;
using System.Drawing;
using System.Globalization;


namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
    public static class PdfFontManager
    {
        public static PdfFont GetFont(float fontSize, PdfFontStyle fontStyle, CultureInfo cultureInfo)
        {
            //             HeiseiKakuGothicW5 The Heisei Kaku Gothic W5 japanese font face.  
            //             HeiseiMinchoW3 The Heisei Mincho W3 japanese font face.  
            //             HanyangSystemsGothicMedium The Hanyang Systems Gothic Medium korean font face.  
            //             HanyangSystemsShinMyeongJoMedium The Hanyang Systems Shin MyeongJo Medium korean font face.  
            //             MonotypeHeiMedium The Monotype Hei Medium chinese traditional font face.  
            //             MonotypeSungLight The Monotype Sung Light chinese traditional font face.  
            //             SinoTypeSongLight The SinoType Song Light chinese simplified font face. 

            PdfFont font;
            switch (cultureInfo.IetfLanguageTag)
            {
                case "zh-CN"://Simplified Chinese
                    font = new PdfCjkStandardFont(PdfCjkFontFamily.SinoTypeSongLight, fontSize, fontStyle);
                    return font;
                case "zh-TW"://Traditional Chinese
                    font = new PdfCjkStandardFont(PdfCjkFontFamily.MonotypeSungLight, fontSize, fontStyle);
                    return font;
                case "ja"://Japanese
                    font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiKakuGothicW5, fontSize, fontStyle);
                    return font;
                case "ko"://Korean
                    font = new PdfCjkStandardFont(PdfCjkFontFamily.HanyangSystemsGothicMedium, fontSize, fontStyle);
                    return font;
            }
            var style = FontStyle.Regular;
            if (fontStyle == PdfFontStyle.Bold)
                style = FontStyle.Bold;
            var theFont = new Font("Helvetica", fontSize, style);
            var pdfFont = new PdfTrueTypeFont(theFont, true);
            return pdfFont;
        }
    }
}
