﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.DayViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.WeekViewModel.js" />
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
	var weekViewModels = {};
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };


	var callWhenAjaxIsCompleted = function (callback) {
		callback();
	};
	if (ajax.CallWhenAllAjaxCompleted)
		callWhenAjaxIsCompleted = ajax.CallWhenAllAjaxCompleted;

	function _initPeriodSelection() {
		var periodData = $('#Preference-body').data('mytime-periodselection');

		selectionViewModel.displayDate(Teleopti.MyTimeWeb.Common.FormatDatePeriod(
				moment($('#Preference-body').data('period-start-date')),
				moment($('#Preference-body').data('period-end-date'))));

		selectionViewModel.nextPeriodDate(moment(periodData.PeriodNavigation.NextPeriod));
		selectionViewModel.previousPeriodDate(moment(periodData.PeriodNavigation.PrevPeriod));
		selectionViewModel.setCurrentDate(moment(periodData.Date));

		var tmpAvailablePreferences = eval($(".preference-split-button").data("mytime-preference-option"));
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

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
			$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ', calendarPlacement: "left", format:  "' + Teleopti.MyTimeWeb.Common.DateFormat + '"}');
			ko.applyBindings(selectionViewModel, $('div.navbar')[1]);
			//ko.applyBindings(selectionViewModel, $('div.navbar')[2]);
		});
	}

	function _deletePreference(successCb) {
		_hideExtendedPanels();
		var dates = [];

		$('#Preference-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				dates.push(date);
			});

		if (dates.length > 0) {
			ajax.Ajax({
				url: "Preference/PreferenceDelete",
				dataType: "json",
				contentType: 'application/json; charset=utf-8',
				type: 'POST',
				data: JSON.stringify({ dateList: dates }),
				success: function (data) {
					_onSuccessDeletePeriod(data, dates, successCb);
				},
				statusCode404: function () { },
			});
		}
	}

	function _onSuccessDeletePeriod(data, dates, successCb) {
		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PreferencePerformanceForMultipleUsers_43322")) {
			data.forEach(function(d){
				preferencesAndScheduleViewModel.DayViewModels[d.Date].ReadPreference(d.Value);
			});
			updateSelectedDatesAndNeighbors(dates);
		} else {
			dates.forEach(function(date) {
				var dayViewModel = preferencesAndScheduleViewModel.DayViewModels[date];
				dayViewModel.ClearPreference(successCb);
				dayViewModel.LoadFeedback();
			});

			periodFeedbackViewModel.LoadFeedback();
			loadNeighborFeedback();
			periodFeedbackViewModel.PossibleNightRestViolations();
		}
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

	function _setPreference(preference,cb) {
		var promises = [];
		addExtendedPreferenceFormViewModel.ValidationError('');

		var validationErrorCallback = function (data) {
			var message = data.Errors.join('</br>');
			addExtendedPreferenceFormViewModel.ValidationError(message);
		};

		if(typeof preference == 'string' && preference.length > 0){
			preference = {
				PreferenceId : preference
			};
		}

		if (preference.SelectedTemplate)
			preference.TemplateName = preference.SelectedTemplate.Text;
		delete preference.AvailableTemplates;
		delete preference.SelectedTemplate;
		delete preference.NewTemplateName;

		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PreferencePerformanceForMultipleUsers_43322")) {
			var postDates = [];
			$('#Preference-body-inner .ui-selected').each(function(index, cell) {
				postDates.push($(cell).data('mytime-date'));
			});
			setSelectedDatesPreferences(postDates, preference, validationErrorCallback);
			periodFeedbackViewModel.LoadFeedback();
			updateSelectedDatesAndNeighbors(postDates);
			periodFeedbackViewModel.PossibleNightRestViolations();
		} else {
			$('#Preference-body-inner .ui-selected').each(function(index, cell) {
				var date = $(cell).data('mytime-date');
				var promise = preferencesAndScheduleViewModel.DayViewModels[date].SetPreference(preference, validationErrorCallback);
				promises.push(promise);
			});
			if (promises.length !== 0) {
				$.when.apply(null, promises).done(function() {
					periodFeedbackViewModel.LoadFeedback();
					loadNeighborFeedback();
					periodFeedbackViewModel.PossibleNightRestViolations();
					if (cb) cb();
				});
			}
		}
	}

	function setSelectedDatesPreferences(postDates, preference, validationErrorCallback){
		preference.Dates = postDates;
		ajax.Ajax({
			url: "Preference/ApplyPreferences",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify(preference),
			success: function(data) {
			    data.forEach(function(d) {
			        preferencesAndScheduleViewModel.DayViewModels[d.Date].ReadPreference(d.Value);
			    });
			},
			error: function(jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				validationErrorCallback(errorMessage);
			}
		});
	}

	function updateSelectedDatesAndNeighbors(postDates){
		var allDates = [];
		$("#Preference-body-inner").find('li[data-mytime-date]').each(function(index, cell) {
			allDates.push($(cell).data('mytime-date'));
		});

		var previousDate, nextDate, getDates = [];
		allDates.forEach(function(date, index) {
			if(postDates.indexOf(date) > -1){
				if (index - 1 > 0)
					previousDate = allDates[index - 1];
			    if (index + 1 < allDates.length)
			        nextDate = allDates[index + 1];

				if(getDates.indexOf(previousDate) == -1)
					getDates.push(previousDate);
				if(getDates.indexOf(nextDate) == -1)
					getDates.push(nextDate);
			}
		});

		ajax.Ajax({
			url: "PeriodPreferenceFeedback/PeriodFeedback",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'GET',
			data: {
				startDate: getDates[0],
				endDate: getDates[getDates.length -1]
			},
			success: function(data) {
				var updatedDatesData = data.filter(function(d){
					return getDates.indexOf(d.Date) > -1 || postDates.indexOf(d.Date) > -1;
				});
				updatedDatesData.forEach(function(d){
					preferencesAndScheduleViewModel.DayViewModels[d.Date].AssignFeedbackData(d);
				});
			}
		});
	}

	function loadNeighborFeedback() {
		var dates = [];
		$("#Preference-body-inner").find('li[data-mytime-date]').each(function (i, e) {
			dates.push($(e).data('mytime-date'));
		});

		$('#Preference-body-inner .ui-selected').each(function (index, ele) {
			var date = $(ele).data('mytime-date');
			var curDateIndex = dates.indexOf(date);

			var previousDayIndex = curDateIndex - 1;
			var nextDayIndex = curDateIndex + 1;

			if (nextDayIndex < dates.length) {
				date = dates[nextDayIndex];
				if (preferencesAndScheduleViewModel.DayViewModels[date])
					preferencesAndScheduleViewModel.DayViewModels[date].LoadFeedback();
			}

			if (previousDayIndex >= 0) {
				date = dates[previousDayIndex];
				if (preferencesAndScheduleViewModel.DayViewModels[date])
					preferencesAndScheduleViewModel.DayViewModels[date].LoadFeedback();
			}
		});
	}

	function _setMustHave(mustHave, successCb) {
		$('#Preference-body-inner .ui-selected')
			.each(function (index, cell) {
				var date = $(cell).data('mytime-date');
				preferencesAndScheduleViewModel.DayViewModels[date].SetMustHave(mustHave, successCb);
			});
	}

	function _setWeeklyWorkTimeWidth() {
		var isShow = $('#showweeklyworktime').val();
		if (isShow == 'True') {
			$("#Preference-body-inner").addClass("preference-body-width");
		}
	}

	function _initAddExtendedButton() {
		var button = $('.Preference-add-extended-button');

		var showMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true';
		addExtendedPreferenceFormViewModel = new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel(ajax, showMeridian, _savePreferenceTemplate, _deletePreferenceTemplate, _setPreference, _isShiftCategorySelectedAsStandardPreference);
		ko.applyBindings(addExtendedPreferenceFormViewModel, $("#Preference-add-extended-form")[0]);
		_loadAvailableTemplates();

		button.click(function (e) {
			addExtendedPreferenceFormViewModel.ToggleAddPreferenceFormVisible();
			e.preventDefault();
		});

		button.attr('data-menu-loaded','true');
	}

	function _loadAvailableTemplates() {
		ajax.Ajax({
			url: "Preference/GetPreferenceTemplates",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				data = data || [];
				data.unshift({ Text: '', Value: '' });
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
		$('.glyphicon-user')
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

	function _initViewModels(loader, callback) {
		var date = portal ? portal.CurrentFixedDate() : null;
		if (date == null) {
			var periodData = $('#Preference-body').data('mytime-periodselection');
			if (periodData) {
				date = periodData.Date;
			} else {
				date = moment().startOf('day').format('YYYY-MM-DD');
			}
		}

		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PreferencePerformanceForMultipleUsers_43322")) {
			_loadPreferenceDataByPeriod();
		} else {
			_assignPreferenceDataToDateCell();
			callback && callback();
		}

		function _loadPreferenceDataByPeriod() {
			var params = {
				startDate: moment(date).subtract(7, 'day').format('YYYY-MM-DD'),
				endDate: moment(date).add(34, 'day').format('YYYY-MM-DD')
			};

			ajax.Ajax({
				url: "PeriodPreferenceFeedback/PeriodFeedback",
				dataType: "json",
				contentType: 'application/json; charset=utf-8',
				type: 'GET',
				data: {
					startDate: params.startDate,
					endDate: params.endDate
				},
				success: function(data) {
					_assignPreferenceDataToDateCell(data);
					callback && callback();
				},
				statusCode404: function() {},
			});
		}

		function _assignPreferenceDataToDateCell(preferenceDataByPeriod) {
			var dayViewModels = {},
				dayViewModelsInPeriod = {};

			$(".header-week-day").each(function(i, f) {
				var s = $(f).data('date');
				$(f).text(moment(s).format('dddd'));
			});

			$('li[data-mytime-week]').each(function(index, element) {
				weekViewModels[index] = new Teleopti.MyTimeWeb.Preference.WeekViewModel(ajax);
				ko.applyBindings(weekViewModels[index], element);
			});

			$('li[data-mytime-date]').each(function(index, element) {
				var dayViewModel, preferenceData;

				if (preferenceDataByPeriod && preferenceDataByPeriod.length > 0) {
					preferenceData = preferenceDataByPeriod.filter(function(p) {
						return p.Date == element.attributes['data-mytime-date'].value;
					});
					dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(_ajaxForDate, preferenceData[0]);
				} else {
					dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel(_ajaxForDate, null);
				}

				dayViewModel.ReadElement(element);
				dayViewModels[dayViewModel.Date] = dayViewModel;
				if ($(element).hasClass("inperiod")) {
					dayViewModelsInPeriod[dayViewModel.Date] = dayViewModel;
				}
				ko.applyBindings(dayViewModel, element);
				if ($('li[data-mytime-week]').length > 0) {
					weekViewModels[Math.floor(index / 7)].DayViewModels.push(dayViewModel);
				}
			});

			var from = $('li[data-mytime-date]').first().data('mytime-date');
			var to = $('li[data-mytime-date]').last().data('mytime-date');

			preferencesAndScheduleViewModel = new Teleopti.MyTimeWeb.Preference
				.PreferencesAndSchedulesViewModel(ajax, dayViewModels);
			selectionViewModel = new Teleopti.MyTimeWeb.Preference.SelectionViewModel(
				dayViewModelsInPeriod,
				$('#Preference-body').data('mytime-maxmusthave'),
				_setMustHave,
				_setPreference,
				_deletePreference,
				$('#Preference-body').data('mytime-currentmusthave'));

			periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference
				.PeriodFeedbackViewModel(ajax, dayViewModelsInPeriod, date, weekViewModels);

			var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
			if (periodFeedbackElement) {
				ko.applyBindings(periodFeedbackViewModel, periodFeedbackElement);
			}

			runLoader = loader ||
				function(call) {
					call();
				};
			runLoader(function() {
				if (preferencesAndScheduleViewModel) {
					preferencesAndScheduleViewModel.LoadPreferencesAndSchedules(from, to)
						.done(function() {
							loadingStarted = true;
							_activateSelectable();
							_activateMeetingTooltip();
							readyForInteraction();
							runLoader(function() {
								periodFeedbackViewModel.LoadFeedback();

								if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PreferencePerformanceForMultipleUsers_43322")) {

									ajax.Ajax({
										url: "Preference/WeeklyWorkTimeSettings",
										dataType: "json",
										type: 'POST',
										contentType: 'application/json; charset=utf-8',
										data: JSON.stringify({
											weekDates: (function(w) {
												var dates = [];
												for (key in w) {
													dates.push(w[key].DayViewModels()[0].Date);
												}
												return dates;
											})(weekViewModels)
										}),
										success: function(data) {
											$.each(weekViewModels, function(index, weekViewModel) {
												weekViewModel.readWeeklyWorkTimeSettings(data.filter(function(d) {
													return d.Date == weekViewModel.DayViewModels()[0].Date;
												})[0]);
											});
										}
									});
								} else {
									if ($('li[data-mytime-week]').length > 0) {
										$.each(weekViewModels,
											function(index, week) {
												week.LoadWeeklyWorkTimeSettings();
											});
									}
									$.each(preferencesAndScheduleViewModel.DayViewModels,
										function(index, day) {
											day.LoadFeedback();
										});
								}
								selectionViewModel.enableDateSelection();
								callWhenAjaxIsCompleted(completelyLoaded);
							});
						});
				}
			});
		}
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
		$('li[data-mytime-week]').each(function (index, element) {
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

			_setWeeklyWorkTimeWidth();
			_initAddExtendedButton();
			_initViewModels(_soon, function(){
				_initPeriodSelection();
				_initExtendedPanels();
			});

		},
		InitViewModels: function () {
			_initViewModels();
		},
		PreferencePartialDispose: function () {
			ajax.AbortAll();
			_cleanBindings();
		},
		DeletePreferenceTemplate: function (templateId) {
			_deletePreferenceTemplate(templateId);
		}
	};

};

Teleopti.MyTimeWeb.Preference = Teleopti.MyTimeWeb.PreferenceInitializer(new Teleopti.MyTimeWeb.Ajax(), Teleopti.MyTimeWeb.Portal);