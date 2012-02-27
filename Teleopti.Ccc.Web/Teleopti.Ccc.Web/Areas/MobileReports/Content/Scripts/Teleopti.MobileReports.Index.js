
if (typeof(Teleopti) === 'undefined') {
	Teleopti = { };

	if (typeof(Teleopti.MobileReports) === 'undefined') {
		Teleopti.MobileReports = { };
	}
}




Teleopti.MobileReports.Index = (function ($) {

	function _init(msgs) {
		var cNs = Teleopti.MobileReports.Common;
		$(document).bind("mobileinit", function () {
			$.mobile.orientationChangeEnabled = false;
			$.mobile.loadingMessage = msgs[0];
			$.mobile.pageLoadErrorMessage = msgs[1];

		});
		_initReportView(cNs);
		_initReportSettingsView(cNs);
		_initHomeView(cNs);
	}
	function _initHomeView(cNs) {
		$("#home-view").live("pageinit", function () {
			$.mobile.changePage($("#report-settings-view"));
		});
	}

	function _initReportSettingsView(cNs) {
		$("#report-settings-view").live("pageinit", function () {
			$('#sel-date').datebox();
			$('#sel-skill').skillpicker();

			(function setDefaultSettings() {
				$('#sel-date').trigger('datebox', { 'method': 'dooffset', 'type': 'y', 'amount': -3 }).trigger('datebox', { 'method': 'doset' });
				setRadioGrpValue('#report-settings-view', 'sel-report');
				$('#report-settings-type-graph').attr('checked', true).checkboxradio("refresh");
			})();

			function setRadioGrpValue(sel, name) {
				var parent = $(sel), val = $('input[name="' + name + '"]:checked', parent).val();
				if (typeof (val) == 'undefined') {
					$('input[name="' + name + '"]', parent).first().attr('checked', true).checkboxradio("refresh");
				}
			}

			$('#report-view-show-button').bind('click', function () {
				$('#report-view').data('report-settings', modelFromSettings()); // Save the values to #report-view data.
				$.mobile.changePage($('#report-view'));

			});

			function modelFromSettings() {
				var parent = $('#report-settings-view');
				var reportRequestParam = {
					"ReportId": $('input[name="sel-report"]:checked', parent).val(),
					"ReportDate": cNs.DateToFixedDate($('#sel-date').data('datebox').theDate),
					"ReportIntervalType": $('input[name="sel-interval"]:checked', parent).val(),
					"SkillSet": ($('#sel-skill').val() || []).join(','),
					"table": $('#report-settings-type-table').is(':checked'),
					"graph": $('#report-settings-type-graph').is(':checked')
				};
				return reportRequestParam;
			}

			function adjustSettingsToData() {
				// set Prefs!
				// {"ReportId": "Id", "ReportDate": "Fixed Date", "ReportIntervalType": "1 / 7", "SkillSet": "-2,3,45,5", "table": "True/False", "graph": "True/False"}
				var parent = $('#report-settings-view');

			}
		});
	}

	function _initReportView(cNs) {
		// make nav widget
		$(window).resize(function () {
			$('#report-view-date-nav-current').width($(window).width() - 110);
		});

		$("#report-view").live("pageinit", function () {
			$('#report-graph-holder').graph();
			$('#report-table-holder').table();
			$('#report-view').bind('report', function () {
				cNs.ClearError();
				var settings = reportDataOrBack();
				if (settings == null) return;

				$.ajax({
					url: 'Report/Report',
					dataType: 'json',
					contentType: 'application/json; charset=utf-8',
					type: 'POST',
					cache: false,
					data: JSON.stringify(settings),
					beforeSend: function () {
						$.mobile.showPageLoadingMsg();
						$('#report-view').find('.ui-btn:not(.ui-disabled)').each(function () {
							$(this).addClass('ui-disabled').addClass('by-me');
						});
					},
					complete: function (jqXHR, textStatus) {
						$('#report-view').find('.ui-btn.by-me').each(function () {
							$(this).removeClass('ui-disabled').removeClass('by-me');
						});
						$.mobile.hidePageLoadingMsg();
					},
					success: function (data, textStatus, jqXHR) {
						updateView(data, settings);
					},
					error: function (jqXHR, textStatus, errorThrown) {
						if (jqXHR.status == 400) {
							var data = $.parseJSON(jqXHR.responseText);
							updateView(data, settings);
							return;
						} else if (jqXHR.status == 500) {
							cNs.AddError('#report-view', 'Server returned an error');
						}
					}
				});
			});

			$('#report-view').live('pageshow', function () {
				$('#report-view').trigger('report');
			});

			function reportDataOrBack() {
				var currentData = $('#report-view').data('report-settings');
				if (!(typeof currentData != 'undefined' && currentData != null)) {
					$.mobile.changePage($('#report-settings-view'));
					return null;
				}
				return currentData;
			}

			function updateView(data, settings) {
				if (typeof data.Errors != 'undefined') {
					cNs.AddError('#report-view', data.Errors.join('<br />'));
					return;
				}
				var graphEnabled = false, tableEnabled = false;

				if (typeof data.Report != 'undefined' && data.Report) {
					var reportInfo = data.Report.ReportInfo;
					if (typeof reportInfo != 'undefined' && reportInfo) {
						$('.ui-btn-text', '#report-view-date-nav-current').html(reportInfo.ReportDate);
						$('#report-name').html(reportInfo.ReportName);
						//$('#report-skillset').html(reportInfo.SkillNames);

						var reportData = data.Report.ReportData;
						graphEnabled = (reportData != null) && settings.graph;
						tableEnabled = (reportData != null) && (settings.table || !settings.graph);
					}
				}
				$('#report-graph-holder').graph('setEnabled', graphEnabled);
				$('#report-graph-holder').graph('setData', data.Report);
				$('#report-graph-holder').graph('refresh');

				$('#report-table-holder').table('setEnabled', tableEnabled);
				$('#report-table-holder').table('setData', data.Report);
				$('#report-table-holder').table('refresh');

			}

			$('#report-view-date-nav-next').bind('click', function () {
				navDate(1);
			});
			$('#report-view-date-nav-prev').bind('click', function () {
				navDate(-1);
			});
			$('#report-view-date-nav-current').bind('swipeleft', function () {
				navDate(1);
			}).bind('swiperight', function () {
				navDate(-1);
			});

			function navDate(dir) {
				var currentData = reportDataOrBack();
				if (currentData == null) return;

				var date = cNs.FixedDateToDate(currentData.ReportDate);
				date.setDate(date.getDate() + (dir * currentData.ReportIntervalType));
				currentData.ReportDate = cNs.DateToFixedDate(date);

				$('#report-view').data('report-settings', currentData);
				$('#report-view').trigger('report');
				$('#report-settings-view').trigger('datachange');
			}
		});
	}

	return {
		Init: function (msgs) {
			_init(msgs);
		}
	};

})(jQuery);