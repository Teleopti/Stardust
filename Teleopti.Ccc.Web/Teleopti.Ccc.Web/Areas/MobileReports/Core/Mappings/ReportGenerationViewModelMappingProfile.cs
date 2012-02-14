using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Mappings
{
	public class ReportGenerationViewModelMappingProfile : Profile
	{
		private readonly Func<IReportDataService> _dataService;
		private readonly Func<IMappingEngine> _mappingEngine;
		private readonly Func<ISkillProvider> _skillProvider;
		private readonly Func<IUserTextTranslator> _userTextTranslator;

		public ReportGenerationViewModelMappingProfile(Func<IMappingEngine> mappingEngine,
		                                               Func<IUserTextTranslator> userTextTranslator,
		                                               Func<ISkillProvider> skillProvider,
		                                               Func<IReportDataService> dataService)
		{
			_mappingEngine = mappingEngine;
			_userTextTranslator = userTextTranslator;
			_skillProvider = skillProvider;
			_dataService = dataService;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<ReportGenerationResult, ReportInfo>()
				.ForMember(d => d.ReportName,
				           o => o.MapFrom(s => _userTextTranslator.Invoke().TranslateText(s.Report.ReportNameResourceKey)))
				.ForMember(d => d.PeriodLegend,
				           o => o.MapFrom(s => s.ReportInput.IntervalType == 1 ? Resources.Time : Resources.Day))
				.ForMember(d => d.ReportDate,
				           o =>
				           o.MapFrom(
				           	s =>
				           		{
				           			var dateOnlyPeriod = s.ReportInput.Period;
				           			return dateOnlyPeriod.StartDate.Equals(dateOnlyPeriod.EndDate)
				           			       	? dateOnlyPeriod.StartDate.ToShortDateString()
				           			       	: dateOnlyPeriod.DateString;
				           		}))
				.ForMember(d => d.SkillNames, o => o.MapFrom(s =>
				                                             	{
				                                             		var list =
				                                             			s.ReportInput.SkillSet.Split(new[] {','},
				                                             			                             StringSplitOptions.RemoveEmptyEntries).
				                                             				ToList();
				                                             		return string.Join(", ",
				                                             		                   _skillProvider.Invoke().GetAvailableSkills().Where
				                                             		                   	(sk =>
				                                             		                   	 list.Contains(
				                                             		                   	 	sk.Id.ToString(CultureInfo.InvariantCulture))).
				                                             		                   	Select(sk => sk.Name).ToArray());
				                                             	}))
				.ForMember(d => d.Y1Legend,
				           o =>
				           o.MapFrom(
				           	s =>
				           	s.Report.LegendResourceKeys[0] == null
				           		? string.Empty
				           		: _userTextTranslator.Invoke().TranslateText(s.Report.LegendResourceKeys[0])))
				.ForMember(d => d.Y2Legend,
				           o =>
				           o.MapFrom(
				           	s =>
				           	s.Report.LegendResourceKeys[1] == null
				           		? string.Empty
				           		: _userTextTranslator.Invoke().TranslateText(s.Report.LegendResourceKeys[1])));


			CreateMap<ReportDataPeriodEntry, ReportTableRowViewModel>()
				.ForMember(d => d.Period, o => o.ResolveUsing(new PeriodValueResolver(_userTextTranslator)))
				.ForMember(d => d.DataColumn1, o => o.MapFrom(s => string.Format("{0:0.0}", s.Y1)))
				.ForMember(d => d.DataColumn2, o => o.MapFrom(s => string.Format("{0:0.0}", s.Y2)));

			CreateMap<ReportGenerationResult, ReportTableRowViewModel[]>()
				.ConvertUsing(
					s =>
					_mappingEngine.Invoke().Map<IEnumerable<ReportDataPeriodEntry>, ReportTableRowViewModel[]>(s.ReportData));


			//_mappingEngine.Invoke().Map<IEnumerable<ReportDataPeriodEntry>,ReportTableRowViewModel[]>(s.ReportData)

			CreateMap<ReportGenerationResult, ReportResponseModel>()
				.ForMember(d => d.Report, o => o.MapFrom(s => s));

			CreateMap<ReportGenerationResult, ReportResponse>()
				.ForMember(d => d.ReportInfo, o => o.MapFrom(s => s))
				.ForMember(d => d.ReportData, o => o.MapFrom(s => s))
				.ForMember(d => d.ReportChart, o => o.UseValue(null));


			/*
			// -2 for all special all skill from Mart
			CreateMap<ReportControlSkillGet, SkillSelectionViewModel>()
				.ForMember(d => d.SkillId, a => a.MapFrom(s => s.Id))
				.ForMember(d => d.SkillName, a => a.MapFrom(s => s.Name))
				.ForMember(d => d.AllSkills, a => a.MapFrom(s => s.Id == -2));
			 * */
		}
	}

	public class PeriodValueResolver : ValueResolver<ReportDataPeriodEntry, string>
	{
		private static readonly string[] WeekDayResources = new[]
		                                                    	{
		                                                    		"ResDayOfWeekMonday", "ResDayOfWeekTuesday",
		                                                    		"ResDayOfWeekWednesday",
		                                                    		"ResDayOfWeekThursday", "ResDayOfWeekFriday",
		                                                    		"ResDayOfWeekSaturday",
		                                                    		"ResDayOfWeekSunday"
		                                                    	};

		private readonly Func<IUserTextTranslator> _userTextTranslator;

		public PeriodValueResolver(Func<IUserTextTranslator>
		                           	textTranslator)
		{
			_userTextTranslator = textTranslator;
		}


		protected override string ResolveCore(ReportDataPeriodEntry source)
		{
			if (WeekDayResources.Contains(source.Period))
			{
				return _userTextTranslator.Invoke().TranslateText(source.Period);
			}

			return source.Period;
		}
	}
}