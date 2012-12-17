/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.debug.js" />

Teleopti.MyTimeWeb.Request.List = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };
	var requestDetailViewModel;
	var pageViewModel;

	function RequestItemViewModel() {

		var self = this;

		self.Subject = ko.observable();
		self.RequestType = ko.observable();
		self.Status = ko.observable();
		self.Dates = ko.observable();
		self.UpdatedOn = ko.observable();
		self.Text = ko.observable();
		self.Link = ko.observable();
		self.Id = ko.observable();
		self.mouseIsOver = ko.observable(false);
		self.isSelected = ko.observable(false);
		self.isLoading = ko.observable(false);

		//TODO: too much gui-info, remove it to be called from the view
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
					setTimeout(function () { self.isLoading(false); }, 2000);
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

		//TODO: expose details instead of wrapping the properties
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

		self.requests = ko.observableArray();

		self.SelectItem = function (requestItemViewModel, event) {
			ko.utils.arrayForEach(self.requests(), function (item) {
				item.isSelected(item == requestItemViewModel);
			});
			requestItemViewModel.ShowDetails(requestItemViewModel, event);
		};

		//TODO: refact to use map & initialize instead
		self.showRequests = function (data) {
			for (var i = 0; i < data.length; i++) {
				self.AddRequest(data[i]);
			}
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
			var update = function (r) {
				var vm = new RequestItemViewModel();
				vm.Initialize(r);
				self.requests.unshift(vm);
			};
			ko.utils.arrayForEach(self.requests(), function (requestVm) {
				if (requestVm.Id() == request.Id) {
					update = function (r) { requestVm.Initialize(r); };
				}
			});
			update(request);
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
		Init: function (readyForInteractionCallback, completelyLoadedCallback, detailViewModel) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			requestDetailViewModel = detailViewModel;
			pageViewModel = new RequestPageViewModel(requestDetailViewModel, readyForInteractionCallback, completelyLoadedCallback);
			_initScrollPaging();
			var element = $('#Requests-body-inner')[0];

			if (element) ko.applyBindings(pageViewModel, element);
		},
		AddItemAtTop: function (request) {
			pageViewModel.AddRequest(request);
		}
	};

})(jQuery);
