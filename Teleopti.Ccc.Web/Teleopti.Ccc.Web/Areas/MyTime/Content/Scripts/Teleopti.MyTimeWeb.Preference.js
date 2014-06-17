/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.PreferencesAndScheduleViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.SelectionViewModel.js" />
/// <reference path="../../../../Content/moment/moment.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}

Teleopti.MyTimeWeb.PreferenceInitializer = function (ajax, portal) {

	var loadingStarted = false;
	var periodFeedbackViewModel = null;
	var addExtendedPreferenceFormViewModel = null;
	var preferencesAndScheduleViewModel = null;
	var selectionViewModel = null;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };


	var callWhenAjaxIsCompleted = function (callback) {
		callback();
	};
	if (ajax.CallWhenAllAjaxCompleted)
		callWhenAjaxIsCompleted = ajax.CallWhenAllAjaxCompleted;

	function _initPeriodSelection() {
		var periodData = $('#Preference-body').data('mytime-periodselection');
	    selectionViewModel.displayDate(periodData.Display);
	    selectionViewModel.nextPeriodDate(moment(periodData.PeriodNavigation.NextPeriod));
	    selectionViewModel.previousPeriodDate(moment(periodData.PeriodNavigation.PrevPeriod));
	    selectionViewModel.setCurrentDate(moment(periodData.Date));

	    var tmpAvailablePreferences = eval($("#preference-split-button").data("mytime-preference-option"));
	    var len = tmpAvailablePreferences != null ? tmpAvailablePreferences.length : 0;
	    var availablePreferences = [];
	    for (var i = 0; i < len; i++) {
	    	var pref = tmpAvailablePreferences[i];
	    	if (pref != undefined) {
	    		availablePreferences.push(pref);
	    	}
	    }
	    selectionViewModel.availablePreferences(availablePreferences);

	    if (availablePreferences && availablePreferences.length > 0) {
	        selectionViewModel.selectedPreference(availablePreferences[0]);
	    }

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
			ko.applyBindings(selectionViewModel, $('div.navbar')[1]);
		});
	}

    function _deletePreference() {
        _hideExtendedPanels();
        var promises = [];
        $('#Preference-body-inner .ui-selected')
            .each(function(index, cell) {
                var date = $(cell).data('mytime-date');
                var promise = preferencesAndScheduleViewModel.DayViewModels[date].DeletePreference();
                promises.push(promise);
            });
        $.when.apply(null, promises)
            .done(function() { periodFeedbackViewModel.LoadFeedback(); });
    }

    function _deletePreferenceTemplate(templateId) {
		ajax.Ajax({
			url: "Preference/PreferenceTemplate/" + templateId,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'Delete',
			success: function (data, textStatus, jqXHR) {
				var templateToDelete = $.grep(addExtendedPreferenceFormViewModel.AvailableTemplates(), function (e) { return e.Value == templateId; })[0];
				addExtendedPreferenceFormViewModel.AvailableTemplates.remove(templateToDelete);
			    addExtendedPreferenceFormViewModel.Reset();
			}
		});
	}

	function _savePreferenceTemplate(preference) {
		addExtendedPreferenceFormViewModel.ValidationError('');
		delete preference.AvailableTemplates;
		delete preference.SelectedTemplate;
		delete preference.SelectedTemplateId;
		var templateData = JSON.stringify(preference);

		ajax.Ajax({
			url: "Preference/PreferenceTemplate",
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			type: 'POST',
			data: templateData,
			statusCode400: function (jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				var message = errorMessage.Errors.join('</br>');
				addExtendedPreferenceFormViewModel.ValidationError(message);
			},
			success: function (data, textStatus, jqXHR) {
				data = data || [];
				addExtendedPreferenceFormViewModel.AvailableTemplates.push(data);
				addExtendedPreferenceFormViewModel.AvailableTemplates.sort(function (l, r) {
					return l.Text > r.Text ? 1 : -1;
				});
				addExtendedPreferenceFormViewModel.SelectedTemplateId(data.Value);
				addExtendedPreferenceFormViewModel.IsSaveAsNewTemplate(false);
			}
		});
	}

	function _setPreference(preference) {
		var promises = [];

		addExtendedPreferenceFormViewModel.ValidationError('');

		var validationErrorCallback = function (data) {
			var message = data.Errors.join('</br>');
			addExtendedPreferenceFormViewModel.ValidationError(message);
		};
		if (preference.SelectedTemplate)
			preference.TemplateName = preference.SelectedTemplate.Text;
		delete preference.AvailableTemplates;
		delete preference.SelectedTemplate;
		delete preference.NewTemplateName;

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

	    var showMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true';
		addExtendedPreferenceFormViewModel = new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel(ajax, showMeridian, _savePreferenceTemplate, _deletePreferenceTemplate, _setPreference, _isShiftCategorySelectedAsStandardPreference);
		ko.applyBindings(addExtendedPreferenceFormViewModel, $("#Preference-add-extended-form")[0]);
		_loadAvailableTemplates();

		button.click(function (e) {
		    addExtendedPreferenceFormViewModel.ToggleAddPreferenceFormVisible();
		    e.preventDefault();
		});
	    
		button.removeAttr('disabled');
	}
    
	function _loadAvailableTemplates() {
		ajax.Ajax({
			url: "Preference/GetPreferenceTemplates",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
			    data = data || [];
			    data.unshift({Text: '', Value: ''});
				addExtendedPreferenceFormViewModel.AvailableTemplates(data);
				addExtendedPreferenceFormViewModel.AvailableTemplates.sort(function (l, r) {
					return l.Text > r.Text ? 1 : -1;
				});
			}
		});
	}
    
    function _isShiftCategorySelectedAsStandardPreference() {
        return $('#Preference-Picker option:selected').data('preference-extended');
    }

	function _activateSelectable() {
		$('#Preference-body-inner').calendarselectable();
	}

	function _activateMeetingTooltip() {
		$('.icon-user')
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

	function _ajaxForDate(model, options) {
	    var type = options.type || 'GET',
		    date = options.date || null, // required
		    data = options.data || {},
		    statusCode400 = options.statusCode400,
		    statusCode404 = options.statusCode404,
		    url = options.url || "Preference/Preference",
		    success = options.success || function () {
		    },
		    complete = options.complete || null;

	    return ajax.Ajax({
	        url: url,
	        dataType: "json",
	        contentType: "application/json; charset=utf-8",
	        type: type,
	        beforeSend: function (jqXHR) {
	            model.AjaxError('');
	            model.IsLoading(true);
	        },
	        complete: function (jqXHR, textStatus) {
	            model.IsLoading(false);
	            if (complete)
	                complete(jqXHR, textStatus);
	        },
	        success: success,
	        data: data,
	        statusCode404: statusCode404,
	        statusCode400: statusCode400,
	        error: function (jqXHR, textStatus, errorThrown) {
	            var error = {
	                ShortMessage: "Error!"
	            };
	            try {
	                error = $.parseJSON(jqXHR.responseText);
	            } catch (e) {
	            }
	            model.AjaxError(error.ShortMessage);
	        }
	    });
	};

	function _initViewModels(loader) {
	    var date = portal ? portal.CurrentFixedDate() : null;
	    if (date == null) {
	        var periodData = $('#Preference-body').data('mytime-periodselection');
	        if (periodData) {
	            date = periodData.Date;
	        } else {
	            date = moment().startOf('day').format('YYYY-MM-DD');
	        }
	    }
	    var dayViewModels = {};

	    var dayViewModelsInPeriod = {};

		$('li[data-mytime-date]').each(function (index, element) {
			var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(_ajaxForDate);
			dayViewModel.ReadElement(element);
			dayViewModels[dayViewModel.Date] = dayViewModel;
			if ($(element).hasClass("inperiod")) {
				dayViewModelsInPeriod[dayViewModel.Date] = dayViewModel;
			}
			ko.applyBindings(dayViewModel, element);
		});
		var from = $('li[data-mytime-date]').first().data('mytime-date');
		var to = $('li[data-mytime-date]').last().data('mytime-date');

		preferencesAndScheduleViewModel = new Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel(ajax, dayViewModels);
		selectionViewModel = new Teleopti.MyTimeWeb.Preference.SelectionViewModel(dayViewModelsInPeriod, $('#Preference-body').data('mytime-maxmusthave'), _setMustHave, _setPreference, _deletePreference);
		periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(ajax, dayViewModelsInPeriod, date);

		var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
		if (periodFeedbackElement) {
			ko.applyBindings(periodFeedbackViewModel, periodFeedbackElement);

		}
			
		loader = loader || function (call) { call(); };
		loader(function () {

			if (preferencesAndScheduleViewModel) {

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
						selectionViewModel.enableDateSelection();
						callWhenAjaxIsCompleted(completelyLoaded);
					});
				});
			}
		});
	}

	function _soon(call) {
		setTimeout(function () { call(); }, 0);
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
    
	function _cleanBindings() {
	    $('li[data-mytime-date]').each(function (index, element) {
	        ko.cleanNode(element);
	    });
	    
	    var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
	    if (periodFeedbackElement)
	        ko.cleanNode(periodFeedbackElement);

	    var mustHaveButton = $('#Preference-must-have-button');
	    if (mustHaveButton.length > 0) {
	        ko.cleanNode(mustHaveButton[0]);
	    }

	    var mustHaveTextElement = $('#Preference-must-have-numbers');
	    if (mustHaveTextElement.length > 0) {
	        ko.cleanNode(mustHaveTextElement[0]);
	    }

	    var navbarElement = $('div.navbar');
	    if (navbarElement.length > 1) {
	        ko.cleanNode(navbarElement[1]);
	    }

	    if (selectionViewModel && selectionViewModel.subscription) {
	        selectionViewModel.subscription.dispose();
	    }
	    selectionViewModel = null;
	    periodFeedbackViewModel = null;
	    preferencesAndScheduleViewModel = null;
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Preference/Index',
				Teleopti.MyTimeWeb.Preference.PreferencePartialInit,
				Teleopti.MyTimeWeb.Preference.PreferencePartialDispose
			);
		},
		PreferencePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			
			if (!$('#Preference-body').length) {
				readyForInteraction();
				completelyLoaded();
				return;
			}
			_initAddExtendedButton();
			_initViewModels(_soon);
			_initPeriodSelection();
			_initExtendedPanels();
			
		},
		InitViewModels: function () {
			_initViewModels();
		},
		PreferencePartialDispose: function () {
			ajax.AbortAll();
		    _cleanBindings();
		},
	    DeletePreferenceTemplate: function(templateId) {
	        _deletePreferenceTemplate(templateId);
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
