Teleopti.MyTimeWeb.PreferenceInitializer = function(ajax, portal) {
	var periodFeedbackViewModel = null;
	var addExtendedPreferenceFormViewModel = null;
	var preferencesAndScheduleViewModel = null;
	var selectionViewModel = null;
	var weekViewModels = {};
	var isHostAMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	var isHostAniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();
	var selectedDateSelectorStr =
		isHostAMobile || isHostAniPad
			? '#Preference-body-inner .date-is-selected'
			: '#Preference-body-inner .ui-selected';
	var readyForInteraction = function() {};
	var completelyLoaded = function() {};

	var callWhenAjaxIsCompleted = function(callback) {
		callback();
	};
	if (ajax.CallWhenAllAjaxCompleted) callWhenAjaxIsCompleted = ajax.CallWhenAllAjaxCompleted;

	function _initPeriodSelection() {
		var periodData = $('#Preference-body').data('mytime-periodselection');

		selectionViewModel.displayDate(
			Teleopti.MyTimeWeb.Common.FormatDatePeriod(
				moment($('#Preference-body').data('period-start-date')),
				moment($('#Preference-body').data('period-end-date'))
			)
		);
		selectionViewModel.nextPeriodDate(moment(periodData.PeriodNavigation.NextPeriod));
		selectionViewModel.previousPeriodDate(moment(periodData.PeriodNavigation.PrevPeriod));
		selectionViewModel.setCurrentDate(moment(periodData.Date));

		var tmpAvailablePreferences = eval($('.preference-split-button').data('mytime-preference-option'));
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
			$('.moment-datepicker').attr(
				'data-bind',
				'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' +
					data.WeekStart +
					', calendarPlacement: "left", format:  "' +
					Teleopti.MyTimeWeb.Common.DateFormat +
					'"}'
			);
			ko.applyBindings(selectionViewModel, $('.preference-toolbar')[0]);
		});
	}

	function _deletePreference(successCb) {
		_hideExtendedPanels();
		var dates = [];

		$(selectedDateSelectorStr).each(function(index, cell) {
			var date = $(cell).data('mytime-date');
			dates.push(date);
		});

		if (dates.length > 0) {
			ajax.Ajax({
				url: 'Preference/PreferenceDelete',
				dataType: 'json',
				contentType: 'application/json; charset=utf-8',
				type: 'POST',
				data: JSON.stringify({ dateList: dates }),
				success: function(data) {
					_onSuccessDeletePeriod(data, dates, successCb);
				},
				statusCode404: function() {}
			});
		}
	}

	function _onSuccessDeletePeriod(data, dates, successCb) {
		data.forEach(function(d) {
			preferencesAndScheduleViewModel.DayViewModels[d.Date].ClearPreference(successCb);
		});
		periodFeedbackViewModel.LoadFeedback();
		updateSelectedDatesAndNeighbors(dates);

		periodFeedbackViewModel.PossibleNightRestViolations();
		_clearSelectedDates();
	}

	function _deletePreferenceTemplate(templateId) {
		ajax.Ajax({
			url: 'Preference/PreferenceTemplate/' + templateId,
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'Delete',
			success: function(data, textStatus, jqXHR) {
				var templateToDelete = $.grep(addExtendedPreferenceFormViewModel.AvailableTemplates(), function(e) {
					return e.Value == templateId;
				})[0];
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
			url: 'Preference/PreferenceTemplate',
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			type: 'POST',
			data: templateData,
			statusCode400: function(jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				var message = errorMessage.Errors.join('</br>');
				addExtendedPreferenceFormViewModel.ValidationError(message);
			},
			success: function(data, textStatus, jqXHR) {
				data = data || [];
				addExtendedPreferenceFormViewModel.AvailableTemplates.push(data);
				addExtendedPreferenceFormViewModel.AvailableTemplates.sort(function(l, r) {
					return l.Text > r.Text ? 1 : -1;
				});
				addExtendedPreferenceFormViewModel.SelectedTemplateId(data.Value);
				addExtendedPreferenceFormViewModel.IsSaveAsNewTemplate(false);
			}
		});
	}

	function _setPreference(preference, cb) {
		addExtendedPreferenceFormViewModel.ValidationError('');

		var validationErrorCallback = function(data) {
			var message = data.Errors && data.Errors.join('</br>');
			addExtendedPreferenceFormViewModel.ValidationError(message);
		};

		if (typeof preference == 'string' && preference.length > 0) {
			preference = {
				PreferenceId: preference
			};
		}

		if (preference.SelectedTemplate) preference.TemplateName = preference.SelectedTemplate.Text;
		delete preference.AvailableTemplates;
		delete preference.SelectedTemplate;
		delete preference.NewTemplateName;

		var postDates = [];
		$(selectedDateSelectorStr).each(function(index, cell) {
			postDates.push($(cell).data('mytime-date'));
		});
		setSelectedDatesPreferences(postDates, preference, validationErrorCallback, cb);
	}

	function setSelectedDatesPreferences(postDates, preference, validationErrorCallback, cb) {
		if (postDates == null || postDates.length === 0) {
			var errorMessage = Teleopti.MyTimeWeb.Common.GetUserTexts().NoDateIsSelected;
			addExtendedPreferenceFormViewModel.ValidationError(errorMessage);
			return;
		}

		preference.Dates = postDates;
		ajax.Ajax({
			url: 'Preference/ApplyPreferences',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify(preference),
			success: function(data) {
				postDates.forEach(function(date) {
					updateMustHaveInMenu(date);
				});
				data.forEach(function(d) {
					preferencesAndScheduleViewModel.DayViewModels[d.Date].ReadPreference(d.Value);
				});
				periodFeedbackViewModel.LoadFeedback();
				updateSelectedDatesAndNeighbors(postDates, cb);
				periodFeedbackViewModel.PossibleNightRestViolations();
				_clearSelectedDates();
			},
			error: function(jqXHR, textStatus, errorThrown) {
				var errorMessage = $.parseJSON(jqXHR.responseText);
				validationErrorCallback(errorMessage);
			}
		});
	}

	function updateMustHaveInMenu(date) {
		var oldMustHave = preferencesAndScheduleViewModel.DayViewModels[date].MustHave();
		selectionViewModel.updateMustHave(false, oldMustHave);
	}

	function updateSelectedDatesAndNeighbors(postDates, cb) {
		var allDates = [];
		$('#Preference-body-inner')
			.find('li[data-mytime-date]')
			.each(function(index, cell) {
				allDates.push($(cell).data('mytime-date'));
			});

		var previousDate,
			nextDate,
			getDates = [];
		allDates.forEach(function(date, index) {
			if (postDates.indexOf(date) > -1) {
				if (index - 1 > 0) previousDate = allDates[index - 1];
				if (index + 1 < allDates.length) nextDate = allDates[index + 1];

				if (getDates.indexOf(previousDate) == -1) getDates.push(previousDate);
				if (getDates.indexOf(nextDate) == -1) getDates.push(nextDate);
			}
		});

		ajax.Ajax({
			url: 'Preference/PeriodFeedback',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'GET',
			data: {
				startDate: getDates[0],
				endDate: getDates[getDates.length - 1]
			},
			success: function(data) {
				var updatedDatesData = data.filter(function(d) {
					return getDates.indexOf(d.Date) > -1 || postDates.indexOf(d.Date) > -1;
				});
				updatedDatesData.forEach(function(d) {
					preferencesAndScheduleViewModel.DayViewModels[d.Date].AssignFeedbackData(d);
				});
				if (cb) cb();
			}
		});
	}

	function _setMustHave(mustHave, successCb) {
		$(selectedDateSelectorStr).each(function(index, cell) {
			var date = $(cell).data('mytime-date');
			preferencesAndScheduleViewModel.DayViewModels[date].SetMustHave(mustHave, function(
				newMustHave,
				originalMustHave
			) {
				successCb && successCb(newMustHave, originalMustHave);
				_clearSelectedDates();
			});
		});
	}

	function _initAddExtendedButton() {
		var button = $('.Preference-add-extended-button');

		var showMeridian = $('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true';
		addExtendedPreferenceFormViewModel = new Teleopti.MyTimeWeb.Preference.AddExtendedPreferenceFormViewModel(
			ajax,
			showMeridian,
			_savePreferenceTemplate,
			_deletePreferenceTemplate,
			_setPreference,
			_isShiftCategorySelectedAsStandardPreference
		);
		ko.applyBindings(addExtendedPreferenceFormViewModel, $('#Preference-add-extended-form')[0]);
		_loadAvailableTemplates();

		button.click(function(e) {
			addExtendedPreferenceFormViewModel.ToggleAddPreferenceFormVisible();
			e.preventDefault();
		});

		button.attr('data-menu-loaded', 'true');
	}

	function _loadAvailableTemplates() {
		ajax.Ajax({
			url: 'Preference/GetPreferenceTemplates',
			dataType: 'json',
			type: 'GET',
			success: function(data, textStatus, jqXHR) {
				data = data || [];
				data.unshift({ Text: '', Value: '' });
				addExtendedPreferenceFormViewModel.AvailableTemplates(data);
				addExtendedPreferenceFormViewModel.AvailableTemplates.sort(function(l, r) {
					return l.Text > r.Text ? 1 : -1;
				});
			}
		});
	}

	function _isShiftCategorySelectedAsStandardPreference() {
		return $('#Preference-Picker option:selected').data('preference-extended');
	}

	function _activateSelectable() {
		if (isHostAMobile || isHostAniPad) {
			$('#Preference-body-inner li[data-mytime-editable="True"]').on('click', function() {
				if (!$(this).hasClass('date-is-selected')) $(this).addClass('date-is-selected');
				else $(this).removeClass('date-is-selected');
			});
		} else {
			$('#Preference-body-inner').calendarselectable();
		}
	}

	function _clearSelectedDates() {
		$('#Preference-body-inner .date-is-selected').removeClass('date-is-selected');
	}

	function _activateMeetingTooltip() {
		$('.glyphicon-user').each(function() {
			var date = $(this)
				.closest('li[data-mytime-date]')
				.attr('data-mytime-date');
			var content = {
				text: $(this).next('.meeting-tooltip')
			};
			$(this).qtip({
				id: 'meeting-' + date,
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
			url = options.url || 'Preference/Preference',
			success = options.success || function() {},
			complete = options.complete || null;

		return ajax.Ajax({
			url: url,
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: type,
			beforeSend: function(jqXHR) {
				model.AjaxError('');
				//model.IsLoading(true);
			},
			complete: function(jqXHR, textStatus) {
				//model.IsLoading(false);
				if (complete) complete(jqXHR, textStatus);
			},
			success: success,
			data: data,
			statusCode404: statusCode404,
			statusCode400: statusCode400,
			error: function(jqXHR, textStatus, errorThrown) {
				var error = {
					ShortMessage: 'Error!'
				};
				try {
					error = $.parseJSON(jqXHR.responseText);
				} catch (e) {}
				model.AjaxError(error.ShortMessage);
			}
		});
	}

	function _initViewModels(loader, callback) {
		var date = portal ? portal.CurrentFixedDate() : null;
		weekViewModels = {};
		if (date == null) {
			var periodData = $('#Preference-body').data('mytime-periodselection');
			if (periodData) {
				date = periodData.Date;
			} else {
				date = moment()
					.startOf('day')
					.format('YYYY-MM-DD');
			}
		}

		_loadPreferenceFeedbackDataByPeriod();

		function _loadPreferenceFeedbackDataByPeriod() {
			var startDate = $('li[data-mytime-date]')[0].attributes['data-mytime-date'].value;
			var params = {
				startDate: moment(startDate).format('YYYY-MM-DD'),
				endDate: moment(startDate)
					.add(42, 'day')
					.format('YYYY-MM-DD')
			};

			ajax.Ajax({
				url: 'Preference/PeriodFeedback',
				dataType: 'json',
				contentType: 'application/json; charset=utf-8',
				type: 'GET',
				data: {
					startDate: params.startDate,
					endDate: params.endDate
				},
				success: function(data) {
					_assignPreferenceFeedbackDataToDateCell(data);
					callback && callback();
				},
				statusCode404: function() {}
			});
		}

		function _assignPreferenceFeedbackDataToDateCell(preferenceDataByPeriod) {
			var dayViewModels = {},
				dayViewModelsInPeriod = {};

			$('.header-week-day').each(function(i, f) {
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
				if ($(element).hasClass('inperiod')) {
					dayViewModelsInPeriod[dayViewModel.Date] = dayViewModel;
				}
				ko.applyBindings(dayViewModel, element);
				if ($('li[data-mytime-week]').length > 0) {
					weekViewModels[Math.floor(index / 7)].DayViewModels.push(dayViewModel);
				}
			});

			var from = $('li[data-mytime-date]')
				.first()
				.data('mytime-date');
			var to = $('li[data-mytime-date]')
				.last()
				.data('mytime-date');

			preferencesAndScheduleViewModel = new Teleopti.MyTimeWeb.Preference.PreferencesAndSchedulesViewModel(
				ajax,
				dayViewModels
			);
			selectionViewModel = new Teleopti.MyTimeWeb.Preference.SelectionViewModel(
				$('#Preference-body').data('mytime-maxmusthave'),
				_setMustHave,
				_setPreference,
				_deletePreference,
				$('#Preference-body').data('mytime-currentmusthave')
			);

			periodFeedbackViewModel = new Teleopti.MyTimeWeb.Preference.PeriodFeedbackViewModel(
				ajax,
				dayViewModelsInPeriod,
				date,
				weekViewModels
			);

			
			function AlertViewModel(feedbackViewModel) {
					var self = this;
					self.PreferenceFeedbackClass = ko.computed(function () {
						return feedbackViewModel.PreferenceFeedbackClass();
					});

					self.WarningCount = ko.computed(function () {
						return feedbackViewModel.WarningCount();
					});

					self.toggleWarningDetail = function () {
						feedbackViewModel.toggleWarningDetail();
					};

					self.AlertDanger= ko.computed(function () {
						return feedbackViewModel.PreferenceFeedbackClass() == 'alert-danger' ? true : false;
					});

					self.IsHostAMobile = feedbackViewModel.IsHostAMobile();
			}

			
			var alertViewModelBinding = new AlertViewModel(periodFeedbackViewModel);
			ko.applyBindings(alertViewModelBinding, $('.warning-indicator')[0]);

			var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
			if (periodFeedbackElement) {
				ko.applyBindings(periodFeedbackViewModel, periodFeedbackElement);
			}

			runLoader =
				loader ||
				function(call) {
					call();
				};
			runLoader(function() {
				if (preferencesAndScheduleViewModel) {
					preferencesAndScheduleViewModel.LoadPreferencesAndSchedules(from, to).done(function() {
						loadingStarted = true;
						_activateSelectable();
						_activateMeetingTooltip();
						readyForInteraction();
						runLoader(function() {
							periodFeedbackViewModel.LoadFeedback();

							loadWeeklyWorkTimeSettings();

							$.each(preferencesAndScheduleViewModel.DayViewModels, function(index, day) {
								day.LoadFeedback();
							});

							selectionViewModel.enableDateSelection();
							callWhenAjaxIsCompleted(completelyLoaded);
						});
					});
				}
			});
		}
	}

	function loadWeeklyWorkTimeSettings() {
		ajax.Ajax({
			url: 'Preference/WeeklyWorkTimeSettings',
			dataType: 'json',
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
					weekViewModel.readWeeklyWorkTimeSettings(
						data.filter(function(d) {
							return d.Date == weekViewModel.DayViewModels()[0].Date;
						})[0]
					);
				});
			}
		});
	}

	function _soon(call) {
		setTimeout(function() {
			call();
		}, 0);
	}

	function _initExtendedPanels() {
		$('.preference .extended-indication').each(function() {
			var date = $(this)
				.closest('li[data-mytime-date]')
				.attr('data-mytime-date');
			$(this).qtip({
				id: 'extended-' + date,
				content: {
					text: $(this).next('.extended-panel')
				},
				position: {
					my: 'top left',
					at: 'top right',
					adjust: {
						x: 4,
						y: 5
					}
				},
				show: {
					event: 'click'
				},
				hide: {
					target: $('#page'),
					event: 'mousedown'
				},
				style: {
					def: false,
					classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
					tip: {
						corner: 'left top'
					}
				}
			});
		});
	}

	function _hideExtendedPanels() {
		$('.preference .extended-indication').qtip('toggle', false);
	}

	function _cleanBindings() {
		$('li[data-mytime-date]').each(function(index, element) {
			ko.cleanNode(element);
		});
		$('li[data-mytime-week]').each(function(index, element) {
			ko.cleanNode(element);
		});

		var periodFeedbackElement = $('#Preference-period-feedback-view')[0];
		if (periodFeedbackElement) ko.cleanNode(periodFeedbackElement);

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
		Init: function() {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'Preference/Index',
				Teleopti.MyTimeWeb.Preference.PreferencePartialInit,
				Teleopti.MyTimeWeb.Preference.PreferencePartialDispose
			);
		},
		PreferencePartialInit: function(readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;

			if (!$('#Preference-body').length) {
				readyForInteraction();
				completelyLoaded();
				return;
			}

			_initAddExtendedButton();
			_initViewModels(_soon, function() {
				_initPeriodSelection();
				_initExtendedPanels();
			});
		},
		InitViewModels: function() {
			_initViewModels();
		},
		PreferencePartialDispose: function() {
			ajax.AbortAll();
			_cleanBindings();
		},
		DeletePreferenceTemplate: function(templateId) {
			_deletePreferenceTemplate(templateId);
		}
	};
};

Teleopti.MyTimeWeb.Preference = Teleopti.MyTimeWeb.PreferenceInitializer(
	new Teleopti.MyTimeWeb.Ajax(),
	Teleopti.MyTimeWeb.Portal
);
