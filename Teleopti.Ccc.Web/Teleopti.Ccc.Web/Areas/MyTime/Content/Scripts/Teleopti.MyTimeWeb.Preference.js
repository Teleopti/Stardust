/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}

Teleopti.MyTimeWeb.PreferenceInitializer = function (ajax, portal) {

	var loadingStarted = false;
	var periodFeedbackViewModel = null;
	var dayViewModels = {};
	var addExtendedTooltip = null;
	var addExtendedPreferenceFormViewModel = null;

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
				var promises = [];
				$('#Preference-body-inner .ui-selected')
					.each(function (index, cell) {
						var date = $(cell).data('mytime-date');
						var promise = dayViewModels[date].DeletePreference();
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
				var promise = dayViewModels[date].SetPreference(preference, validationErrorCallback);
				promises.push(promise);
			});
		if (promises.length != 0) {
			$.when.apply(null, promises)
				.done(function () { periodFeedbackViewModel.LoadFeedback(); });
		}

	}

	function _initAddExtendedButton() {
		var button = $('#Preference-add-extended-button');
		var template = $('#Preference-add-extended-form');
		addExtendedPreferenceFormViewModel = new AddExtendedPreferenceFormViewModel();

		addExtendedTooltip = $('<div/>')
			.qtip({
				id: "add-extended",
				content: {
					text: template
				},
				position: {
					target: button,
					my: "left top",
					at: "left bottom",
					adjust: {
						x: 5,
						y: -5
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

						$('#Preference-extended-apply')
							.button()
							.click(function () {
								_setPreference(ko.toJS(addExtendedPreferenceFormViewModel));
							});

						ko.applyBindings(addExtendedPreferenceFormViewModel, $("#Preference-add-extended-form")[0]);

					}
				}
			});
	}

	function _initMustHaveButton() {
		$('#Preference-must-have-button')
			.click(function () {
				_setPreference(true);
			});
	}

	function _hideAddExtendedTooltip() {
		addExtendedTooltip.qtip('toggle', false);
	}

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	function _initViewModels(loader) {

		dayViewModels = {};
		$('li[data-mytime-date].inperiod').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			ko.applyBindings(dayViewModel, element);
		});

		var date = portal ? portal.CurrentFixedDate() : null;
		periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(ajax, dayViewModels, date);

		var element = $('#Preference-period-feedback-view')[0];
		if (element)
			ko.applyBindings(periodFeedbackViewModel, element);

		loader = loader || function (call) { call(); };
		loader(function () {
			periodFeedbackViewModel.LoadFeedback();
			$.each(dayViewModels, function (index, element) {
				element.LoadPreference(function () {
					element.LoadFeedback();
				});
				loadingStarted = true;
			});
		});
	}

	function _callWhenLoaded(callback) {
		if (loadingStarted && !ajax.IsRequesting())
			callback();
		else
			setTimeout(function () { _callWhenLoaded(callback); }, 100);
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
		PreferencePartialInit: function () {
			if (!$('#Preference-body').length)
				return;
			_initPeriodSelection();
			_initViewModels(_soon);
			_activateSelectable();
			_initExtendedPanels();
		},
		PreferencePartialDispose: function () {
			_hideAddExtendedTooltip();
		},
		CallWhenLoaded: function (callback) {
			_callWhenLoaded(callback);
		}
	};

};

Teleopti.MyTimeWeb.Preference = Teleopti.MyTimeWeb.PreferenceInitializer(Teleopti.MyTimeWeb.Ajax, Teleopti.MyTimeWeb.Portal);

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
