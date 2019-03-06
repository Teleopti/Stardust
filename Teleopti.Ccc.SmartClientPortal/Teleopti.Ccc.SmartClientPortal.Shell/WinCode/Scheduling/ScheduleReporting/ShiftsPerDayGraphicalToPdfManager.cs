using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public class ShiftsPerDayGraphicalToPdfManager
	{
		private string _reportTitle;
		private readonly IDictionary<IPerson, string> _persons;
		private readonly CultureInfo _culture;
		private DateOnlyPeriod _period;
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly bool _rightToLeft;
		public const int NoteWidth = 50;
		public const int NameWidth = 130;
		private float _nameDateWidth;
		private const int NameDateWithPadding = 10;
		private readonly ScheduleReportDialogGraphicalModel _model;


		public ShiftsPerDayGraphicalToPdfManager(CultureInfo culture, IDictionary<IPerson, string> persons, DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, bool rightToLeft, ScheduleReportDialogGraphicalModel model)
		{
			_culture = culture;
			_persons = persons;
			_period = period;
			_stateHolder = stateHolder;
			_rightToLeft = rightToLeft;
			_model = model;
		}

		public static IList<IPerson> SortOnTime(IEnumerable<IScheduleDay> scheduleDays, bool startTime)
		{
			if(scheduleDays == null)
				throw new ArgumentNullException("scheduleDays");

			var sortedPersons = new List<IPerson>();
			IList<IPerson> noLayerPersons = new List<IPerson>();

			var projections = new List<Tuple<IVisualLayerCollection, IPerson>>();

			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService();
				var visualLayerCollection = projection.CreateProjection();

				if (visualLayerCollection.Period().HasValue)
					projections.Add(new Tuple<IVisualLayerCollection, IPerson>(visualLayerCollection, scheduleDay.Person));
				else
					noLayerPersons.Add(scheduleDay.Person);
			}

			if (!projections.IsEmpty())
			{
				if (startTime)
				{
					var sorted = from p in projections
								 orderby p.Item1.Period().Value.StartDateTime
								 select p;


					sortedPersons.AddRange(sorted.Select(projection => projection.Item2));
				}
				else
				{
					var sorted = from p in projections
								 orderby p.Item1.Period().Value.EndDateTime
								 select p;

					sortedPersons.AddRange(sorted.Select(projection => projection.Item2));
				}
			}

			sortedPersons.AddRange(noLayerPersons);

			return sortedPersons;
		}

		public static IList<IPerson> SortOnCommonAgentName(IDictionary<IPerson, string> persons)
		{
			var sorted = from kvp in persons
			             orderby kvp.Value
			             select kvp.Key;

			return sorted.ToList();
		}

		private IEnumerable<IScheduleDay> ScheduleDays(DateOnly dateOnly)
		{
			var dic = _stateHolder.Schedules;

			return _persons.Keys.Select(person => dic[person].ScheduledDay(dateOnly)).ToList();
		}

		public PdfDocument ExportTeamView()
		{
			var doc = new PdfDocument{PageSettings = {Orientation = PdfPageOrientation.Landscape}};
			IList<IPerson> sortedList = new List<IPerson>();

			if (_model.SortOnAgentName)
				sortedList = SortOnCommonAgentName(_persons);

			foreach (var dateOnly in _period.DayCollection())
			{
				PdfPage page;
				var top = NewPageTeamView(doc, dateOnly, 10, true, _rightToLeft, _culture, out page);

				if(_model.SortOnStartTime || _model.SortOnEndTime)
					sortedList = SortOnTime(ScheduleDays(dateOnly), _model.SortOnStartTime);

				foreach (var person in sortedList)
				{
					if (top > page.GetClientSize().Height - 30)
						top = NewPageTeamView(doc, dateOnly, 10, true, _rightToLeft, _culture, out page);

					var dic = _stateHolder.Schedules;
					var part = dic[person].ScheduledDay(dateOnly);

					var nameDate = new ScheduleReportGraphicalDrawNameDate(page, top, _persons[part.Person], _rightToLeft, _culture);
					nameDate.DrawData((int)_nameDateWidth);

					var drawSchedule = new ScheduleReportGraphicalDrawSchedule(page, (int)_nameDateWidth + NameDateWithPadding, NoteWidth, (int)top, part, _rightToLeft, _model.ShowPublicNote, _culture);
					var drawTimeline = new ScheduleReportGraphicalDrawTimeline(_culture, _rightToLeft, 0, page, 0, 0);
				    top = drawSchedule.Draw(drawTimeline.TimelinePeriod(_period.DayCollection(), _stateHolder, _persons.Keys.ToList(), dateOnly));
				}
			}

			AddHeader(doc);
			AddFooter(doc);
			return doc;
		}

		public PdfDocument ExportAgentView(ITimeZoneGuard timeZoneGuard)
		{
			var doc = new PdfDocument { PageSettings = { Orientation = PdfPageOrientation.Landscape } };
			var sortedList = SortOnCommonAgentName(_persons);
			
			foreach (var person in sortedList)
			{
				var previousDateOnly = _period.DayCollection()[0];
	
				PdfPage page;
				var top = NewPageAgentView(doc, _rightToLeft, _culture, out page, person, _period.StartDate);
				
				foreach (var dateOnly in _period.DayCollection())
				{
					if (top > page.GetClientSize().Height - 30)
						top = NewPageAgentView(doc, _rightToLeft, _culture, out page, person, dateOnly);
					else if(AddPageForDaylightSavingTime(previousDateOnly, dateOnly, timeZoneGuard.CurrentTimeZone()))
						top = NewPageAgentView(doc, _rightToLeft, _culture, out page, person, dateOnly);

					var dic = _stateHolder.Schedules;
					var part = dic[person].ScheduledDay(dateOnly);

					var nameDate = new ScheduleReportGraphicalDrawNameDate(page, top, dateOnly.Date.ToString("d", _culture), _rightToLeft, _culture);
					nameDate.DrawData((int)_nameDateWidth);

					var drawSchedule = new ScheduleReportGraphicalDrawSchedule(page, (int)_nameDateWidth + NameDateWithPadding, NoteWidth, (int)top, part, _rightToLeft, _model.ShowPublicNote, _culture);
					var drawTimeline = new ScheduleReportGraphicalDrawTimeline(_culture, _rightToLeft, 0, page, 0, 0);
				    top = drawSchedule.Draw(drawTimeline.TimelinePeriod(_period.DayCollection(), _stateHolder, _persons.Keys.ToList(), dateOnly));

					previousDateOnly = dateOnly;

				}
			}

			AddHeader(doc);
			AddFooter(doc);
			return doc;
		}

		public static bool AddPageForDaylightSavingTime(DateOnly previousDateOnly, DateOnly dateOnly, TimeZoneInfo timeZone)
		{
			if (timeZone.IsDaylightSavingTime(previousDateOnly.Date.AddDays(1)) != timeZone.IsDaylightSavingTime(dateOnly.Date.AddDays(1)))
				return true;

			return timeZone.IsDaylightSavingTime(previousDateOnly.Date.AddDays(2)) != timeZone.IsDaylightSavingTime(dateOnly.Date.AddDays(2));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting.ScheduleReportDrawHeader.#ctor(Syncfusion.Pdf.PdfPage,System.String,System.Globalization.CultureInfo)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting.ScheduleReportDrawHeader.#ctor(Syncfusion.Pdf.PdfPage,System.String,System.Boolean,System.Globalization.CultureInfo)")]
		private float NewPageTeamView(PdfDocument doc, DateOnly dateOnly, float top, bool newReport, bool rightToLeft, CultureInfo culture, out PdfPage page)
		{
			if (doc == null)
				throw new ArgumentNullException("doc");

			page = doc.Pages.Add();
			_reportTitle = Resources.ShiftsPerDayHeader + " " + dateOnly.Date.ToString("d", culture);

			if (newReport)
			{
				var drawHeader = new ScheduleReportDrawHeader(page, _reportTitle, culture);
				top = drawHeader.Draw();
			}

			var drawNameDate = new ScheduleReportGraphicalDrawNameDate(page, top, Resources.Name, rightToLeft,culture);
			drawNameDate.DrawHeader(GetMaxLengthStringPersons(_persons));
			_nameDateWidth = drawNameDate.ColumnWidth;

			var drawTimeLine = new ScheduleReportGraphicalDrawTimeline(_culture, _rightToLeft, (int)top, page, (int)_nameDateWidth + NameDateWithPadding, NoteWidth);
			top = drawTimeLine.Draw(_period.DayCollection(), _stateHolder, _persons.Keys.ToList(), dateOnly);
			
			return top;
		}

		private float NewPageAgentView(PdfDocument doc, bool rightToLeft, CultureInfo culture, out PdfPage page, IPerson person, DateOnly dateOnly)
		{
			if(doc == null)
				throw new ArgumentNullException("doc");

			if(person == null)
				throw new ArgumentNullException("person");

			page = doc.Pages.Add();
			_reportTitle = _persons[person];

			//if (newReport)
			var drawHeader = new ScheduleReportDrawHeader(page, _reportTitle, culture);
			var top = drawHeader.Draw();

			var drawNameDate = new ScheduleReportGraphicalDrawNameDate(page, top, Resources.Date, rightToLeft, culture);
			drawNameDate.DrawHeader(GetMaxLengthStringDates(_period.DayCollection()));
			_nameDateWidth = drawNameDate.ColumnWidth;

			var drawTimeLine = new ScheduleReportGraphicalDrawTimeline(_culture, _rightToLeft, (int)top, page, (int)_nameDateWidth + NameDateWithPadding, NoteWidth);
			top = drawTimeLine.Draw(_period.DayCollection(), _stateHolder, _persons.Keys.ToList(), dateOnly);

			return top;		
		}

		private void AddHeader(PdfDocument doc)
		{
			var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

			//Create page template
			var header = new PdfPageTemplateElement(rect);
			var brush = new PdfSolidBrush(Color.Gray);

			var font = PdfFontManager.GetFont(6f, PdfFontStyle.Regular, _culture);
			var format = new PdfStringFormat
			{
				RightToLeft = _rightToLeft,
				LineAlignment = PdfVerticalAlignment.Top,
				Alignment = PdfTextAlignment.Right
			};
			//Create page number field
			var pageNumber = new PdfPageNumberField(font, brush);

			//Create page count field
			var count = new PdfPageCountField(font, brush);

            var createdDate = new PdfCreationDateField(font, brush);
            createdDate.DateFormatString = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;

			var compositeField = new PdfCompositeField(font, brush, Resources.CreatedPageOf, createdDate, pageNumber, count)
			{
				StringFormat = format,
				Bounds = header.Bounds
			};
			compositeField.Draw(header.Graphics);
			//Add header template at the top.
			doc.Template.Top = header;
		}

		private void AddFooter(PdfDocument doc)
		{
			var rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

			//Create a page template
			var footer = new PdfPageTemplateElement(rect);

			var brush = new PdfSolidBrush(Color.Gray);
			var font = PdfFontManager.GetFont(6, PdfFontStyle.Bold, _culture);

			var format = new PdfStringFormat
			{
				RightToLeft = _rightToLeft,
				Alignment = PdfTextAlignment.Center,
				LineAlignment = PdfVerticalAlignment.Bottom
			};
			footer.Graphics.DrawString(Resources.PoweredByTeleoptCCC, font, brush, rect, format);

			doc.Template.Bottom = footer;

		}

		public static string GetMaxLengthStringPersons(IDictionary<IPerson, string> persons)
		{
			if(persons == null)
				throw  new ArgumentNullException("persons");

			var maxWidth = string.Empty;

			foreach (var s in persons.Values)
			{
				if (s.Length >= maxWidth.Length)
					maxWidth = s;
			}
	
			return maxWidth;
		}

		public string GetMaxLengthStringDates(IList<DateOnly> dates)
		{
			if(dates == null)
				throw new ArgumentNullException("dates");

			var maxWidth = string.Empty;

			foreach (var dateOnly in dates)
			{
				if (dateOnly.Date.ToString("d", _culture).Length >= maxWidth.Length)
					maxWidth = dateOnly.Date.ToString("d", _culture);
			}

			return maxWidth;
		}
	}
}
