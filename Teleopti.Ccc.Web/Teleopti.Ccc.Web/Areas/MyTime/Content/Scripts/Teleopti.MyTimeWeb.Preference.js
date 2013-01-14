/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.PreferencesAndScheduleViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}

Teleopti.MyTimeWeb.PreferenceInitializer = function (ajax, portal) {

	var loadingStarted = false;
	var periodFeedbackViewModel = null;
	var addExtendedTooltip = null;
	var addExtendedPreferenceFormViewModel = null;
	var preferencesAndScheduleViewModel = null;
	var mustHaveCountViewModel = null;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };


	var callWhenAjaxIsCompleted = function (callback) {
		callback();
	};
	if (ajax.CallWhenAllAjaxCompleted)
		callWhenAjaxIsCompleted = ajax.CallWhenAllAjaxCompleted;



	function _initPeriodSelection() {
		var rangeSelectorId = '#PreferenceDateRangeSelector';
		var periodData = $('#Preference-body').data('mytime-periodselection');
		portal.InitPeriodSelection(rangeSelectorId, periodData);
	}

	function _initSplitButton() {
		$('#Preference')
			.splitbutton({
				clicked: function (event, item) {
					_setPreference(item.value);
				}
			});
	}

	function _initDeleteButton() {
		$('#Preference-delete-button')
			.click(function () {
				_hideExtendedPanels();
				var promises = [];
				$('#Preference-body-inner .ui-selected')
					.each(function (index, cell) {
						var date = $(cell).data('mytime-date');
						var promise = preferencesAndScheduleViewModel.DayViewModels[date].DeletePreference();
						promises.push(promise);
					});
				$.when.apply(null, promises)
					.done(function () { periodFeedbackViewModel.LoadFeedback(); });
			})
			.removeAttr('disabled');
	}

	function _setPreference(preference) {
		var promises = [];

		addExtendedPreferenceFormViewModel.ValidationError('');

		var validationErrorCallback = function (data) {
			var message = data.Errors.join('</br>');
			addExtendedPreferenceFormViewModel.ValidationError(message);
		};
		$('#Preference-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				var promise = preferencesAndScheduleViewModel.DayViewModels[date].SetPreference(preference, validationErrorCallback);
				promises.push(promise);
			});
		if (promises.length != 0) {
			$.when.apply(null, promises)
				.done(function () { periodFeedbackViewModel.LoadFeedback(); });
		}

	}

	function _setMustHave(mustHave) {
		$('#Preference-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				preferencesAndScheduleViewModel.DayViewModels[date].SetMustHave(mustHave);
			});
	}

	function _initAddExtendedButton() {
		var button = $('#Preference-add-extended-button');
		var template = $('#Preference-add-extended-form');
		addExtendedPreferenceFormViewModel = new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel();

		addExtendedTooltip = $('<div/>')
			.qtip({
				id: "add-extended",
				content: {
					text: template,
					title: {
						text: '&nbsp;',
						button: 'Close'
					}
				},
				position: {
					target: button,
					my: "left top",
					at: "left bottom",
					adjust: {
						x: 11,
						y: 0
					}
				},
				show: {
					target: button,
					event: 'click'
				},
				hide: {
					target: button,
					event: 'click'
				},
				style: {
					def: false,
					classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
					tip: {
						corner: "top left"
					}
				},
				events: {
					render: function () {
						$('#Preference-extended-reset')
							.button()
							.click(function () {
								addExtendedPreferenceFormViewModel.reset();
							});
						$('#Preference-extended-apply')
							.button()
							.click(function () {
								_setPreference(ko.toJS(addExtendedPreferenceFormViewModel));
							});

						ko.applyBindings(addExtendedPreferenceFormViewModel, $("#Preference-add-extended-form")[0]);

					}
				}
			});

		button.removeAttr('disabled');
	}

	function _initMustHaveButton() {
		$('#Preference-must-have-button')
			.click(function () {
				_setMustHave(true);
			})
			.removeAttr("disabled")
			;
		$('#Preference-must-have-delete-button')
			.click(function () {
				_setMustHave(false);
			})
			.removeAttr("disabled")
			;
	}

	function _hideAddExtendedTooltip() {
		addExtendedTooltip.qtip('toggle', false);
	}

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	function _activateMeetingTooltip() {
		$('.meeting-small')
			.each(function () {
				var date = $(this).closest("li[data-mytime-date]").attr("data-mytime-date");
				var content = {
					text: $(this).next('.meeting-tooltip')
				};
				$(this).qtip({
					id: "meeting-" + date,
					content: content,
					style: {
						def: false,
						classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
						tip: true
					},
					position: {
						my: 'bottom left',
						at: 'top right',
						target: 'mouse',
						adjust: {
							x: 10,
							y: -13
						}
					}
				});
			});
	}

	function _initViewModels(loader) {
		var date = portal ? portal.CurrentFixedDate() : null;

		var dayViewModels = {};
		var dayViewModelsInPeriod = {};
		$('li[data-mytime-date]').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			if($(element).hasClass("inperiod")) {
				dayViewModelsInPeriod[dayViewModel.Date] = dayViewModel;
			}
			ko.applyBindings(dayViewModel, element);
		});
		var from = $('li[data-mytime-date]').first().data('mytime-date');
		var to = $('li[data-mytime-date]').last().data('mytime-date');

		preferencesAndScheduleViewModel = new Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel(ajax, dayViewModels);
		mustHaveCountViewModel = new Teleopti.MyTimeWeb.Preference.MustHaveCountViewModel(dayViewModelsInPeriod, $('#Preference-body').data('mytime-maxmusthave'));
		periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(ajax, dayViewModelsInPeriod, date);

		var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
		if (periodFeedbackElement)
			ko.applyBindings(periodFeedbackViewModel, periodFeedbackElement);

		var mustHaveTextElement = $('#Preference-must-have-numbers');
		if (mustHaveTextElement.length > 0) {
			mustHaveTextElement.attr("data-bind", "text: MustHaveText");
			ko.applyBindings(mustHaveCountViewModel, mustHaveTextElement[0]);
		}

		loader = loader || function (call) { call(); };
		loader(function () {
			preferencesAndScheduleViewModel.LoadPreferencesAndSchedules(from, to)
				.done(function () {
					loadingStarted = true;
					_activateSelectable();
					_activateMeetingTooltip();
					readyForInteraction();
					loader(function () {
						periodFeedbackViewModel.LoadFeedback();
						$.each(preferencesAndScheduleViewModel.DayViewModels, function (index, day) {
							day.LoadFeedback();
						});
						callWhenAjaxIsCompleted(completelyLoaded);
					});
				});
		});
	}

	function _soon(call) {
		setTimeout(function () { call(); }, 300);
	}

	function _initExtendedPanels() {
		$('.preference .extended-indication')
			.each(function () {
				var date = $(this).closest("li[data-mytime-date]").attr("data-mytime-date");
				$(this)
					.qtip(
						{
							id: "extended-" + date,
							content: {
								text: $(this).next('.extended-panel')
							},
							position: {
								my: "top left",
								at: "top right",
								adjust: {
									x: 4,
									y: 5
								}
							},
							show: {
								event: 'click'
							},
							hide: {
								target: $("#page"),
								event: 'mousedown'
							},
							style: {
								def: false,
								classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
								tip: {
									corner: "left top"
								}
							}
						});
			});

	}

	function _hideExtendedPanels() {
		$('.preference .extended-indication')
			.qtip('toggle', false);
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Preference/Index',
				Teleopti.MyTimeWeb.Preference.PreferencePartialInit,
				Teleopti.MyTimeWeb.Preference.PreferencePartialDispose
			);
			_initSplitButton();
			_initDeleteButton();
			_initAddExtendedButton();
			_initMustHaveButton();
		},
		InitViewModels: function () {
			_initViewModels();
		},
		PreferencePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			if (!$('#Preference-body').length) {
				readyForInteraction();
				completelyLoaded();
				return;
			}
			_initPeriodSelection();
			_initViewModels(_soon);
			_initExtendedPanels();
		},
		PreferencePartialDispose: function () {
			_hideAddExtendedTooltip();
			ajax.AbortAll();
		}
	};

};

Teleopti.MyTimeWeb.Preference = Teleopti.MyTimeWeb.PreferenceInitializer(new Teleopti.MyTimeWeb.Ajax(), Teleopti.MyTimeWeb.Portal);

Teleopti.MyTimeWeb.Preference.formatTimeSpan = function (totalMinutes) {
	if (!totalMinutes)
		return "0:00";
	var minutes = totalMinutes % 60;
	var hours = Math.floor(totalMinutes / 60);
	return hours + ":" + Teleopti.MyTimeWeb.Preference.rightPadNumber(minutes, "00");
};

Teleopti.MyTimeWeb.Preference.rightPadNumber = function (number, padding) {
	var formattedNumber = padding + number;
	var start = formattedNumber.length - padding.length;
	formattedNumber = formattedNumber.substring(start);
	return formattedNumber;
};
