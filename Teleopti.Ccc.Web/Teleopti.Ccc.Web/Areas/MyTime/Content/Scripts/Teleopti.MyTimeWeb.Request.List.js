/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.debug.js" />

Teleopti.MyTimeWeb.Request.List = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var pageViewModel;

	function RequestItemViewModel() {

		var self = this;

		self.Subject = ko.observable();
		self.RequestType = ko.observable();
		self.RequestPayload = ko.observable();
		self.Status = ko.observable();
		self.Dates = ko.observable();
		self.UpdatedOn = ko.observable();
		self.Text = ko.observable();
		self.Link = ko.observable();
		self.Id = ko.observable();
		self.mouseIsOver = ko.observable(false);
		self.isSelected = ko.observable(false);
		self.isLoading = ko.observable(false);
		self.CanDelete = ko.observable(true);

		self.Type = ko.computed(function () {
			var payload = (self.RequestPayload() != '') ? ', ' + self.RequestPayload() : '';
			return self.RequestType() + payload;
		});

		self.ShowDetails = function (viewmodel, event) {
			var distanceFromTop = Math.max(15, $(event.currentTarget).position().top - 30);
			ajax.Ajax({
				url: self.Link(),
				dataType: "json",
				type: 'GET',
				beforeSend: function () {
					self.isLoading(true);
				},
				complete: function () {
					self.isLoading(false);
				},
				success: function (data) {
					Teleopti.MyTimeWeb.Request.RequestDetail.ShowRequest(data, distanceFromTop);
				}
			});
		};

		self.toggle = function () {
			self.mouseIsOver(!self.mouseIsOver());
		};

	}


	function RequestPageViewModel(requestDetailViewModel, readyForInteraction, completelyLoaded) {

		var self = this;

		self.ready = readyForInteraction;
		self.completed = completelyLoaded;
		self.details = ko.observable(requestDetailViewModel);

		//henke remove wrapper...
		self.AddTextRequest = function() {
			return requestDetailViewModel.AddTextRequest;
		};

		self.AddAbsenceRequest = function () {
			return requestDetailViewModel.AddAbsenceRequest;
		};

		requestDetailViewModel.isUpdate.subscribe(function (newValue) {
			if (!newValue) self.setSelected(null);
		});

		self.isUpdate = ko.computed(function () {
			return requestDetailViewModel.isUpdate();
		});

		self.AbsenceRequestTabVisible = ko.computed(function () {
			return requestDetailViewModel.AbsenceRequestTabVisible();
		});

		self.TabSeparatorVisible = ko.computed(function () {
			return requestDetailViewModel.TabSeparatorVisible();
		});

		self.IsFullDay = ko.computed(function () {
			return requestDetailViewModel.IsFullDay();
		});

		self.TextRequestTabVisible = ko.computed(function () {
			return requestDetailViewModel.TextRequestTabVisible();
		});

		self.Template = ko.computed(function () {
			return requestDetailViewModel.Template();
		});

		self.requests = ko.observableArray();

		self.SelectItem = function (requestItemViewModel, event) {
			self.setSelected(requestItemViewModel);
			requestItemViewModel.ShowDetails(requestItemViewModel, event);
		};

		self.setSelected = function (requestItemViewModel) {
			ko.utils.arrayForEach(self.requests(), function (item) {
				item.isSelected(item == requestItemViewModel);
			});
		};

		self.moreToLoad = ko.observable(false);

		self.showRequests = function (data) {
			ko.utils.arrayForEach(data, function (item) {
				var vm = new RequestItemViewModel();
				vm.Initialize(item);
				self.requests.push(vm);
			});
		};

		self.Delete = function (requestItemViewModel) {

			var url = requestItemViewModel.Link();
			ajax.Ajax({
				url: url,
				dataType: "json",
				contentType: 'application/json; charset=utf-8',
				type: "DELETE",
				success: function () {
					self.requests.remove(requestItemViewModel);
				},
				error: function (jqXHR, textStatus) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				}
			});
		};

		self.AddRequest = function (request) {
			var selectedViewModel = ko.utils.arrayFirst(self.requests(), function (item) {
				return item.Id() == request.Id;
			});

			if (selectedViewModel) {
				self.requests.remove(selectedViewModel);
			}
			else {
				selectedViewModel = new RequestItemViewModel();
				selectedViewModel.Initialize(request);
			}

			ajax.Ajax({
				url: selectedViewModel.Link(),
				dataType: "json",
				type: 'GET',
				success: function (data) {
					selectedViewModel.Initialize(data);
					self.requests.unshift(selectedViewModel);
					self.setSelected(selectedViewModel);
				}
			});
		};

		self.loadPage = function () {
			var skip = self.requests().length;
			var take = 20;
			ajax.Ajax({
				url: "Requests/Requests",
				dataType: "json",
				type: 'GET',
				data: {
					Take: take,
					Skip: skip
				},
				success: function (data) {
					self.moreToLoad(data.length == take);
					self.showRequests(data);
				},
				complete: function () {
					if (self.ready) {
						self.ready();
						self.ready = null;
					}
					if (self.completed) {
						self.completed();
						self.completed = null;
					}
				}
			});
		};
	}

	ko.utils.extend(RequestItemViewModel.prototype, {
		Initialize: function (data) {
			var self = this;
			self.Subject(data.Subject);
			self.RequestType(data.Type);
			self.Status(data.Status);
			self.Dates(data.Dates);
			self.UpdatedOn(data.UpdatedOn);
			self.Text(data.Text);
			self.Link(data.Link.href);
			self.Id(data.Id);
			self.RequestPayload(data.Payload);
			self.CanDelete(data.Link.Methods.indexOf("DELETE") != -1);
		}
	});

	function _initScrollPaging() {
		pageViewModel.loadPage();
		$(window).scroll(_loadAPageIfRequired);
	}

	function _loadAPageIfRequired() {
		var jqWindow = $(window);
		var jqDocument = $(window.document);
		if (_isAtBottom(jqDocument, jqWindow)) {
			pageViewModel.loadPage();
		}
	}

	function _isAtBottom(jqDocument, jqWindow) {

		var totalContentHeight = jqDocument.height();
		var inViewContentHeight = jqWindow.height();
		var aboveViewContentHeight = jqWindow.scrollTop();
		return totalContentHeight - inViewContentHeight - aboveViewContentHeight <= 0;
	}

	return {
		Init: function (detailViewModel, readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			pageViewModel = new RequestPageViewModel(detailViewModel, readyForInteractionCallback, completelyLoadedCallback);

			_initScrollPaging();
			var element = $('#Requests-body-inner')[0];

			if (element) ko.applyBindings(pageViewModel, element);
		},
		AddItemAtTop: function (request) {
			pageViewModel.AddRequest(request);
		}
	};

})(jQuery);

