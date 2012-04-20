/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Preference = (function ($) {

	var _feedbackLoadingStarted = false;

	function _initPeriodSelection() {
		var rangeSelectorId = '#PreferenceDateRangeSelector';
		var periodData = $('#Preference-body').data('mytime-periodselection');
		Teleopti.MyTimeWeb.Portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _initSplitButton() {
		$('#Preference-set-button')
			.parent()
			.splitbutton({
				clicked: function (event, item) {
					$('#Preference-body-inner .ui-selected')
						.each(function (index, cell) {
							var date = $(cell).data('mytime-date');
							_ajax({
								type: 'POST',
								data: {
									Date: date,
									Id: item.value
								},
								date: date,
								fillData: _fillPreference,
								complete: function () {
									_loadFeedbackForDate(date);
								}
							});
						});
				}
			});
	}

	function _initDeleteButton() {
		$('#Preference-delete-button')
			.click(function () {
				$('#Preference-body-inner .ui-selected')
					.each(function (index, cell) {
						var date = $(cell).data('mytime-date');
						_ajax({
							type: 'DELETE',
							data: { Date: date },
							date: date,
							statusCode404: function () { },
							fillData: _fillPreference,
							complete: function () {
								_loadFeedbackForDate(date);
							}
						});
					});
			})
			.removeAttr('disabled')
			;
	}

	function _loadFeedbackSoon() {
		_feedbackLoadingStarted = false;
		setTimeout(function () { _loadFeedback(); }, 300);
	}

	function _loadFeedback() {
		$('li[data-mytime-date].feedback').each(function () {
			var date = $(this).data('mytime-date');
			_loadFeedbackForDate(date);
			_feedbackLoadingStarted = true;
		});
	}

	function _loadFeedbackForDate(date) {
		_ajax({
			url: "PreferenceFeedback/Feedback",
			type: 'GET',
			data: { Date: date },
			date: date,
			fillData: _fillFeedback
		});
	}

	function _fillPreference(cell, data) {
		$('li[data-mytime-date="' + data.Date + '"] .day-content').css("border-left-color", data.HexColor);

		var preference = $('.preference', cell);
		preference.text(data.PreferenceRestriction || "");
	}

	function _fillFeedback(cell, data) {
		var error = $('.feedback-error', cell);
		error.text(data.FeedbackError || "");

		var possibleStartTimes = $('.possible-start-times', cell);
		possibleStartTimes.text(data.PossibleStartTimes || "");
		var possibleEndTimes = $('.possible-end-times', cell);
		possibleEndTimes.text(data.PossibleEndTimes || "");
		var possibleContractTimes = $('.possible-contract-times', cell);
		possibleContractTimes.text(data.PossibleContractTimes || "");
	}

	function _ajax(options) {

		var type = options.type || 'GET',
		    date = options.date || null, // required
		    data = options.data || {},
		    statusCode404 = options.statusCode404,
			url = options.url || "Preference/Preference",
		    fillData = options.fillData || function () { },
		    complete = options.complete || null
			;

		var cell = $('li[data-mytime-date="' + date + '"]');
		$.myTimeAjax({
			url: url,
			dataType: "json",
			type: type,
			beforeSend: function (jqXHR) {

				var currentRequest = cell.data('request');
				if (currentRequest) {
					currentRequest.abort();
				}

				cell.data('request', jqXHR);

				$('#loading-small-gray-blue')
					.clone()
					.removeAttr('id')
					.removeClass('template')
					.addClass('loading-small-gray-blue')
					.addClass('temporary')
					.appendTo(cell)
					;

			},
			complete: function (jqXHR, textStatus) {

				cell.data('request', null);

				$('.temporary', cell)
					.remove();

				if (complete)
					complete(jqXHR, textStatus);
			},
			data: data,
			success: function (data, textStatus, jqXHR) {
				fillData(cell, data);
			},
			statusCode404: statusCode404,
			error: function (jqXHR, textStatus, errorThrown) {

				if (textStatus == "abort")
					return;

				var cellHtml = $('<h2></h2>')
					.addClass('error');

				$('.preference', cell)
					.html(cellHtml);

				var error = "Error!";
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {
					cellHtml.append(error);
					return;
				}

				$('<a></a>')
					.append(error.ShortMessage + "!")
					.click(function () {
						errorInfo.toggle();
					})
					.appendTo(cellHtml)
					;
				var errorInfo = $('<div></div>')
					.hide()
					.width(300)
					.css({
						'position': 'absolute',
						'border': '1px solid #ccc',
						'background-color': 'white',
						'padding': '10px'
					})
					.append(error.Message)
					.insertAfter(cellHtml)
					;

			}
		});
	}

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Preference/Index', Teleopti.MyTimeWeb.Preference.PreferencePartialInit);
			_initSplitButton();
			_initDeleteButton();
		},
		PreferencePartialInit: function () {
			_initPeriodSelection();
			_activateSelectable();
			_loadFeedbackSoon();
		},
		FeedbackIsLoaded: function () {
			return _feedbackLoadingStarted && !Teleopti.MyTimeWeb.Ajax.IsRequesting();
		}
	};

})(jQuery);

$(function () { Teleopti.MyTimeWeb.Preference.Init(); });
