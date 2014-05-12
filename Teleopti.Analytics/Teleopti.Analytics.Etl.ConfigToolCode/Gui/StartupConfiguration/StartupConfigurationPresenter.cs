using System;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigToolCode.Gui.StartupConfiguration
{
	public class StartupConfigurationPresenter
	{
		private readonly IStartupConfigurationView _view;
		private readonly StartupConfigurationModel _model;
		readonly ILog _logger = LogManager.GetLogger(typeof(StartupConfigurationPresenter));

		public StartupConfigurationPresenter(IStartupConfigurationView view, StartupConfigurationModel model)
		{
			_view = view;
			_model = model;
		}

		public IBaseConfiguration ConfigurationToSave { get; set; }

		public void Initialize()
		{
			_view.LoadCultureList(_model.CultureList);
			_view.LoadIntervalLengthList(_model.IntervalLengthList);
			_view.LoadTimeZoneList(_model.TimeZoneList);
			SetDefaultCulture();
			SetDefaultIntervalLength();
			SetDefaultTimeZone();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void SetDefaultTimeZone()
		{
			if (string.IsNullOrEmpty(_model.OriginalConfiguration.TimeZoneCode))
			{
				_view.SetDefaultTimeZone(_model.GetTimeZoneItem(TimeZoneInfo.Local));
			}
			else
			{
				try
				{
					var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_model.OriginalConfiguration.TimeZoneCode);
					_view.SetDefaultTimeZone(_model.GetTimeZoneItem(timeZone));
				}
				catch (Exception ex)
				{
					_logger.WarnFormat(CultureInfo.InvariantCulture, "The ETL TimeZoneId configured in db was invalid: '{0}'. ExceptionMessage: '{1}'.",
					                   _model.OriginalConfiguration.TimeZoneCode, ex.Message);
					_view.SetDefaultTimeZone(_model.GetTimeZoneItem(TimeZoneInfo.Local));
				}
				
			}
			
		}

		private void SetDefaultIntervalLength()
		{
			var defaultIntervalLength = 15;

			if (_model.IntervalLengthAlreadyInUse.HasValue)
			{
				defaultIntervalLength = (int)_model.IntervalLengthAlreadyInUse;
				_view.DisableIntervalLength();
			}
			else if (_model.OriginalConfiguration.IntervalLength.HasValue && _model.IntervalLengthList.Any(item => item.Id == _model.OriginalConfiguration.IntervalLength.Value))
				defaultIntervalLength = _model.OriginalConfiguration.IntervalLength.Value;

			_view.SetDefaultIntervalLength(defaultIntervalLength);
			ValidateIntervalLength(defaultIntervalLength);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Analytics.Etl.ConfigToolCode.StartupConfiguration.IStartupConfigurationView.ShowErrorMessage(System.String)")]
		private void ValidateIntervalLength(int intervalLength)
		{
			if (_view.SelectedIntervalLengthValue != null)
				return;

			_view.DisableOkButton();
			_view.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, "Interval Length already in use is {0} minutes. Supported Interval Length are 10, 15, 30 and 60 minutes.", intervalLength));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void SetDefaultCulture()
		{
			var defaultCultureInfo = CultureInfo.CurrentCulture.FixPersianCulture();

			if (_model.OriginalConfiguration.CultureId.HasValue)
			{
				try
				{
					defaultCultureInfo = CultureInfo.GetCultureInfo(_model.OriginalConfiguration.CultureId.Value).FixPersianCulture();
				}
				catch
				{
					_logger.DebugFormat(CultureInfo.InvariantCulture, "The ETL Culture LCID configured in db was invalid: '{0}'.",
					                    _model.OriginalConfiguration.CultureId.Value);
				}
				
			}

			_view.SetDefaultCulture(_model.GetCultureItem(defaultCultureInfo));
		}

		public void Save(int cultureId, int intervalLengthMinutes, string timeZoneId)
		{
			ConfigurationToSave = new BaseConfiguration(cultureId, intervalLengthMinutes, timeZoneId);
			_model.SaveConfiguration(ConfigurationToSave);
		}
	}
}