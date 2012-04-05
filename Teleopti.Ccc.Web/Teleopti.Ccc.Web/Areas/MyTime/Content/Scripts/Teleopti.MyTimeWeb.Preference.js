/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Preference = (function ($) {

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
								date: date
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
							statusCode404: function () { }
						});
					});
			})
			.removeAttr('disabled')
			;
	}

	function _ajax(args) {
		var type = args.type || 'GET',
		    date = args.date || null, // required
		    data = args.data || {},
		    statusCode404 = args.statusCode404
			;
		$.myTimeAjax({
				url: "Preference/Preference",
				dataType: "json",
				type: type,
				beforeSend: function() {
					var cell = $('li[data-mytime-date="' + date + '"]')
						.addClass('relative')
						;
					$('<div>')
						.addClass('loading overlay')
						.appendTo(cell)
						.show()
						;
				},
				complete: function(jqXHR, textStatus) {
					$('li[data-mytime-date="' + date + '"]').removeClass('relative');
					$('li[data-mytime-date="' + date + '"] .overlay').remove();
				},
				data: data,
				success: function(data, textStatus, jqXHR) {
					var cell = $('li[data-mytime-date="' + data.Date + '"]');
					cell.removeClassStartingWith('color_');
					cell.addClass(data.StyleClassName);
					
					var preference = $('.preference', cell);
					preference.text(data.PreferenceRestriction || "");

					var error = $('.error', cell);
					error.text(data.FeedbackError || "");
					
					var possibleStartTimes = $('.possible-start-times', cell);
					possibleStartTimes.text(data.PossibleStartTimes || "");
					var possibleEndTimes = $('.possible-end-times', cell);
					possibleEndTimes.text(data.PossibleEndTimes || "");
					var possibleContractTimes = $('.possible-contract-times', cell);
					possibleContractTimes.text(data.PossibleContractTimes || "");
				},
				statusCode404: statusCode404,
				error: function(jqXHR, textStatus, errorThrown) {
					var cellHtml = $('<h2></h2>')
						.addClass('error');
					$('li[data-mytime-date="' + date + '"] .preference')
						.html(cellHtml);
					try {
						var error = $.parseJSON(jqXHR.responseText);
						$('<a></a>')
							.append(error.ShortMessage + "!")
							.click(function() {
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
					} catch(e) {
						cellHtml.append("Error!");
					}
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
		}
	};

})(jQuery);

$(function () { Teleopti.MyTimeWeb.Preference.Init(); });

