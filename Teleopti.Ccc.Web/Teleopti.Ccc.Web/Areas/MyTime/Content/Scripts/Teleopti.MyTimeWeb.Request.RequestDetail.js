﻿Teleopti.MyTimeWeb.Request.RequestDetail = (function($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax(),
		parentViewModel = null,
		defaultDateTimes = null,
		weekStart = 3,
		overtimeRequestDetailViewModel;

	function RequestDetailParentViewModel(weekStart, baseUtcOffsetInMinutes, daylightSavingAdjustment) {
		var self = this;
		self.requestViewModel = ko.observable();
		self.baseUtcOffsetInMinutes = baseUtcOffsetInMinutes;
		self.daylightSavingAdjustment = daylightSavingAdjustment;

		self.createRequestViewModel = function(callback) {
			var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel(
				_addRequest,
				callback,
				weekStart,
				defaultDateTimes
			);
			ajax.Ajax({
				url: 'Requests/PersonalAccountPermission',
				dataType: 'json',
				type: 'GET',

				success: function(data) {
					vm.readPersonalAccountPermission(data);
				}
			});
			return vm;
		};

		self.createShiftTradeRequestViewModel = function(id) {
			var shiftTradeRequestDetailViewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel(
				ajax
			);
			shiftTradeRequestDetailViewModel.loadIsEditMessageEnabled();
			shiftTradeRequestDetailViewModel.setMiscSetting(id);
			self.requestViewModel(shiftTradeRequestDetailViewModel);
		};

		self.addAbsenceRequestDetail = function(requestViewModel) {
			Teleopti.MyTimeWeb.Request.AbsenceRequestDetailViewModel(requestViewModel, ajax);
		};

		self.createShiftExchangeOfferViewModel = function(data) {
			var shiftExchangeOfferDetailViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(
				ajax,
				_addItemAtTop
			).Update(data);
			shiftExchangeOfferDetailViewModel.DateFormat(_datePickerFormat());
			self.requestViewModel(shiftExchangeOfferDetailViewModel);
		};

		self.CancelAddingNewRequest = function() {
			Teleopti.MyTimeWeb.Request.List.HideRequests(false);
			Teleopti.MyTimeWeb.Request.HideFab(false);

			self.requestViewModel(null);
			overtimeRequestDetailViewModel = null;
			Teleopti.MyTimeWeb.Request.ResetToolbarActiveButtons();
			Teleopti.MyTimeWeb.Request.ActiveRequestList();
		};
	}

	function _addItemAtTop(data) {
		Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
		parentViewModel.CancelAddingNewRequest();
	}

	function _datePickerFormat() {
		return Teleopti.MyTimeWeb.Common.DateFormat;
	}

	function _hideOthers() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeBulletinBoard();
	}

	function _prepareForAddingRequest() {
		_hideOthers();
		var model = parentViewModel.createRequestViewModel(_addItemAtTop);

		model.DateFormat(_datePickerFormat());
		parentViewModel.requestViewModel(model);
	}

	function _addTextRequestClick() {
		Teleopti.MyTimeWeb.Request.List.HideRequests(true);
		_prepareForAddingRequest();
		parentViewModel.requestViewModel().AddTextRequest(true);
	}

	function _addAbsenceRequestClick() {
		Teleopti.MyTimeWeb.Request.List.HideRequests(true);
		_prepareForAddingRequest();
		parentViewModel.requestViewModel().AddAbsenceRequest(true);
	}

	function _addPostShiftForTradeClick(date) {
		Teleopti.MyTimeWeb.Request.List.HideRequests(true);
		_hideOthers();
		var defaultTime = {
			defaultStartTime: defaultDateTimes.defaultStartTime,
			defaultEndTime: defaultDateTimes.defaultEndTime
		};
		var model = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _addItemAtTop).Create(
			defaultTime
		);
		model.DateFormat(_datePickerFormat());
		parentViewModel.requestViewModel(model);

		var tomorrow = moment()
			.startOf('day')
			.add('days', 1);
		var requestDay = moment(date);
		if (requestDay.isBefore(tomorrow)) requestDay = tomorrow;
		model.DateFrom(requestDay);
		model.DateTo(requestDay);
	}

	function _databindModel(requestDetailViewModel) {
		parentViewModel = requestDetailViewModel;
		var element = $('#Request-add-data-binding-area')[0];
		if (element) {
			ko.cleanNode(element);
			ko.applyBindings(parentViewModel, element);
		}
	}

	function _addRequest(model, successCallback, errorCallback) {
		model.ShowError(false);

		var formData = _getFormData(model);
		ajax.Ajax({
			url: formData.Url,
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify(formData),
			success: function(data, textStatus, jqXHR) {
				Teleopti.MyTimeWeb.Request.List.HideRequests(false);
				Teleopti.MyTimeWeb.Request.HideFab(false);
				model.IsPostingData(false);
				model.IsNewInProgress(false);
				model.Subject('');
				model.Message('');

				if (successCallback != undefined) {
					successCallback(data);
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				if (jqXHR.status === 400) {
					var data = $.parseJSON(jqXHR.responseText);
					model.ErrorMessage(data.Errors.join('</br>'));
					model.ShowError(true);

					if (errorCallback != undefined) {
						errorCallback(jqXHR, textStatus, errorThrown);
					}
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);

				if (errorCallback != undefined) {
					errorCallback(jqXHR, textStatus, errorThrown);
				}
			}
		});
	}

	function _createOvertimeRequestViewModel(data, parentViewModel, callback) {
		var isViewingDetail = true;
		var disableGetDefaultStartTime = true;
		overtimeRequestDetailViewModel = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(
			ajax,
			callback,
			parentViewModel,
			weekStart,
			isViewingDetail,
			disableGetDefaultStartTime
		);
		overtimeRequestDetailViewModel.Initialize(data);
	}

	function _setRequest(data, callback) {
		if (data.TypeEnum == 2) {
			parentViewModel.createShiftTradeRequestViewModel(data.Id);
			parentViewModel.requestViewModel().Initialize(data);
			parentViewModel.requestViewModel().loadSwapDetails();
		} else if (data.TypeEnum == 3) {
			parentViewModel.createShiftExchangeOfferViewModel(data);
		} else if (data.TypeEnum == 4) {
			_createOvertimeRequestViewModel(data, parentViewModel, callback);
		} else {
			var model = parentViewModel.createRequestViewModel(callback);
			model.IsUpdate(true);
			model.DateFormat(_datePickerFormat());

			parentViewModel.requestViewModel(model);
			parentViewModel.requestViewModel().Initialize(data);

			if (data.TypeEnum == 1) {
				parentViewModel.addAbsenceRequestDetail(model);
			}
		}
	}

	function _enableDisableDetailSection(data) {
		//TODO: for OVERTIME requests
		if (data.TypeEnum == 4) {
			if (data.Link.Methods.indexOf('PUT') == -1) {
				overtimeRequestDetailViewModel.IsEditable(false);
				overtimeRequestDetailViewModel.showUseDefaultStartTimeToggle(false);
			} else {
				overtimeRequestDetailViewModel.IsEditable(true);
				overtimeRequestDetailViewModel.showUseDefaultStartTimeToggle(true);
			}
		} else {
			if (data.Link.Methods.indexOf('PUT') == -1) {
				parentViewModel.requestViewModel().IsEditable(false);
			} else {
				parentViewModel.requestViewModel().IsEditable(true);
			}
		}
	}

	function _getFormData(model) {
		var absenceId = model.AbsenceId();
		if (absenceId == undefined) {
			absenceId = null;
		}

		return {
			Url: model.TypeEnum() == 0 ? 'Requests/TextRequest' : 'Requests/AbsenceRequest',
			Subject: model.Subject(),
			AbsenceId: absenceId,
			Period: {
				StartDate: Teleopti.MyTimeWeb.Common.FormatServiceDate(model.DateFrom()),
				StartTime: model.TimeFrom(),
				EndDate: Teleopti.MyTimeWeb.Common.FormatServiceDate(model.DateTo()),
				EndTime: model.TimeTo()
			},
			Message: model.Message(),
			EntityId: model.EntityId(),
			FullDay: model.IsFullDay()
		};
	}

	function _prepareForViewModel(object) {
		defaultDateTimes = object;
	}

	function _addOvertimeRequest() {
		_hideOthers();
		var isViewingDetail = false;
		var overtimeRequestViewModel = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(
			ajax,
			_addItemAtTop,
			parentViewModel,
			isViewingDetail
		);
		parentViewModel.requestViewModel(overtimeRequestViewModel);
		Teleopti.MyTimeWeb.Request.List.HideRequests(true);
	}

	return {
		Init: function(ajaxobj) {
			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();

			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
				weekStart = data.WeekStart;

				parentViewModel = new RequestDetailParentViewModel(
					data.WeekStart,
					data.BaseUtcOffsetInMinutes,
					data.DaylightSavingTimeAdjustment
				);
				_databindModel(parentViewModel);
			});
		},
		ShowRequest: function(data, callback) {
			_setRequest(data, callback);
			_enableDisableDetailSection(data);

			//Jianfeng TODO: we dont have to assign viewmodel to parentViewModel.requestViewModel() and empty
			//it only for returning. This should be refactored but only handle OVERTIME requests in new way for now.
			if (data.TypeEnum == 4) {
				return overtimeRequestDetailViewModel;
			} else {
				var detailModel = parentViewModel.requestViewModel();
				parentViewModel.requestViewModel(null);
				return detailModel;
			}
		},
		AddTextRequestClick: function() {
			_addTextRequestClick();
		},
		AddAbsenceRequestClick: function() {
			_addAbsenceRequestClick();
		},
		AddPostShiftForTradeClick: function(date) {
			_addPostShiftForTradeClick(date);
		},
		HideNewTextOrAbsenceRequestView: function() {
			parentViewModel.requestViewModel(null);
		},
		AddTextOrAbsenceRequest: function(model, successCallback, errorCallback) {
			_addRequest(model, successCallback, errorCallback);
		},
		PrepareForViewModel: function(object) {
			_prepareForViewModel(object);
		},
		AddOvertimeRequest: function() {
			_addOvertimeRequest();
		},
		CancelAddingNewRequest: function() {
			parentViewModel.CancelAddingNewRequest();
		},
		GetFormData: function(model) {
			return _getFormData(model);
		},
		ParentViewModel: function() {
			return parentViewModel;
		}
	};
})(jQuery);
