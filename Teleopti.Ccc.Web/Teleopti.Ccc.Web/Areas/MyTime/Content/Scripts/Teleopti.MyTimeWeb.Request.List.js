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

	var scrollCount = 0;
	//TODO: move bindings to separate js-file
	ko.bindingHandlers.fadeInIf = {
		update: function (element, valueAccessor, allBindingsAccessor) {
			var value = valueAccessor(), allBindings = allBindingsAccessor();

			var valueUnwrapped = ko.utils.unwrapObservable(value);

			var fadeInOpacity = allBindings.fadeInOpacity || 1.0;
			var fadeOutOpacity = allBindings.fadeOutOpacity || 0.1;
			var fadeInDuration = allBindings.fadeInDuration || 300;
			var fadeOutDuration = allBindings.fadeOutDuration || 300;
			var hiddenWhenFalse = allBindings.hiddenWhenFalse || false;

			$(element).stop();
			if (valueUnwrapped) {
				if (hiddenWhenFalse) {
					$(element).show();
				}
				$(element).animate({ opacity: fadeInOpacity }, fadeInDuration);
			}
			else
				$(element).animate({ opacity: fadeOutOpacity }, fadeOutDuration, function () {
					if (hiddenWhenFalse) {
						$(element).hide();
					}
				});
		}
	};

	ko.bindingHandlers.increaseWidthIf = {
		update: function (element, valueAccessor, allBindingsAccessor) {
			var value = valueAccessor(), allBindings = allBindingsAccessor();
			if (!element.initialWidthForIncreaseIfBinding) {
				element.initialWidthForIncreaseIfBinding = $(element).width();
			}
			var valueUnwrapped = ko.utils.unwrapObservable(value);

			var increaseBy = allBindings.increaseBy || 20;
			var increaseDuration = allBindings.fadeInDuration || 150;
			var decreaseDuration = allBindings.fadeOutDuration || 150;
			$(element).stop();

			if (valueUnwrapped)
				$(element).animate({ width: element.initialWidthForIncreaseIfBinding + increaseBy }, decreaseDuration);
			else
				$(element).animate({ width: element.initialWidthForIncreaseIfBinding }, increaseDuration);
		}
	};

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };

	var noMoreToLoad;
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

	function RequestPageViewModel(requestDetailViewModel) {

		var self = this;

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

		self.loadData = function () {
			//TODO: put initialization of data here
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
		_loadAPage();
		$(window).scroll(_loadAPageIfRequired);
	}

	function _loadAPageIfRequired() {
		if (!_hasMoreToLoad())
			return;
		var jqWindow = $(window);
		var jqDocument = $(window.document);
		if (_isAtBottom(jqDocument, jqWindow)) {
			_loadAPage();
		}
	}

	function _isAtBottom(jqDocument, jqWindow) {
		var totalContentHeight = jqDocument.height();
		var inViewContentHeight = jqWindow.height();
		var aboveViewContentHeight = jqWindow.scrollTop();
		return totalContentHeight - inViewContentHeight - aboveViewContentHeight <= 0;
	}

	function _loadAPage() {
		if (pageViewModel) var skip = pageViewModel.requests.length;
		var take = 20;
		ajax.Ajax({
			url: "Requests/Requests",
			dataType: "json",
			type: 'GET',
			beforeSend: _loading,
			data: {
				Take: take,
				Skip: skip
			},
			success: function (data, textStatus, jqXHR) {
				pageViewModel.showRequests(data);
				if (data.length == 0 || data.length < take) {
					_noMoreToLoad();
				} else {
					_moreToLoad();
				}
			},
			complete: function () {
				if (readyForInteraction)
					readyForInteraction();
				readyForInteraction = null;
				if (completelyLoaded)
					completelyLoaded();
				completelyLoaded = null;
			}
		});
	}

	function _hasMoreToLoad() {
		return $('.request-list .arrow-down').is(':visible');
	}

	function _loading() {
		$('.request-list .arrow-down').hide();
		$('.request-list .loading-gradient').show();
	}

	function _noMoreToLoad() {
		noMoreToLoad = true;
		$('.request-list .arrow-down').hide();
		$('.request-list .loading-gradient').hide();
	}

	function _moreToLoad() {
		$('.request-list .arrow-down').show();
		$('.request-list .loading-gradient').hide();
	}

	function _checkScroll() {
		$(window).scroll(function () {
			_loadAPageIfRequired();
		});
	}

	return {
		Init: function (readyForInteractionCallback, completelyLoadedCallback, detailViewModel) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			requestDetailViewModel = detailViewModel;
			pageViewModel = new RequestPageViewModel(requestDetailViewModel);
			_checkScroll();
			_initScrollPaging();
			var element = $('#Requests-body-inner')[0];

			if (element) ko.applyBindings(pageViewModel, element);
		},
		AddItemAtTop: function (request) {
			pageViewModel.AddRequest(request);
		}
	};

})(jQuery);
