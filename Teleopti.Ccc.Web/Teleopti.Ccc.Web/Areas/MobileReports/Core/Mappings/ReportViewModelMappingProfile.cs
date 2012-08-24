namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Mappings
{
	using System;

	using AutoMapper;

	using Domain.Common;
	using Domain.WebReport;
	using Providers;
	using Models.Domain;
	using Models.Layout;
	using Interfaces.Domain;

	public class ReportViewModelMappingProfile : Profile
	{
		private readonly Func<IDateBoxGlobalizationViewModelFactory> _dateBoxGlobalizationViewModelFactory;
		private readonly Func<IDefinedReportProvider> _definedReportProvider;
		private readonly Func<ISkillProvider> _skillProvider;
		private readonly Func<IUserTextTranslator> _userTextTranslator;

		public ReportViewModelMappingProfile(
			Func<IUserTextTranslator> userTextTranslator,
			Func<IDefinedReportProvider> definedReportProvider,
			Func<ISkillProvider> skillProvider,
			Func<IDateBoxGlobalizationViewModelFactory> dateBoxGlobalizationViewModelFactory)
		{
			_userTextTranslator = userTextTranslator;
			_definedReportProvider = definedReportProvider;
			_skillProvider = skillProvider;
			_dateBoxGlobalizationViewModelFactory = dateBoxGlobalizationViewModelFactory;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, ReportViewModel>().ForMember(
				d => d.Reports, o => o.MapFrom(s => _definedReportProvider.Invoke().GetDefinedReports())).ForMember(
					d => d.Skills, o => o.MapFrom(s => _skillProvider.Invoke().GetAvailableSkills())).ForMember(
						d => d.DateBoxGlobalization,
						o => o.MapFrom(s => _dateBoxGlobalizationViewModelFactory.Invoke().CreateDateBoxGlobalizationViewModel()));

			CreateMap<DefinedReportInformation, ReportSelectionViewModel>().ForMember(
				d => d.ReportId, a => a.MapFrom(s => s.ReportId)).ForMember(
					d => d.ReportName, a => a.MapFrom(s => _userTextTranslator.Invoke().TranslateText(s.ReportNameResourceKey)));

			// -2 for all special all skill from Mart
			CreateMap<ReportControlSkillGet, SkillSelectionViewModel>()
				.ForMember(d => d.SkillId, a => a.MapFrom(s => s.Id))
				.ForMember(d => d.SkillName, a => a.MapFrom(s => s.Name))
				.ForMember(d => d.AllSkills, a => a.MapFrom(s => s.Id == -2));
		}
	}
}