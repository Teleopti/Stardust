﻿/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>
/// <reference path="jquery.ui.connector.js"/>

Teleopti.MyTimeWeb.Request.List = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };

	var requestDetailViewModel;
	var pageViewModel;

	function RequestItemViewModel(request) {
		var self = this;

		self.Subject = ko.observable(request.Subject);
		self.RequestType = ko.observable(request.Type);
		self.Status = ko.observable(request.Status);
		self.Dates = ko.observable(request.Dates);
		self.UpdatedOn = ko.observable(request.UpdatedOn);
		self.Text = ko.observable(request.Text);
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

		self.example = ko.observable("tell Henke to remove this!!...");
		self.changeExample = function () {
			self.example("really... tell Henke to remove this!!");
		};

		self.requests = ko.observableArray();

		//TODO: refact to use map instead
		self.showRequests = function (data) {
			for (var i = 0; i < data.length; i++) {
				self.AddRequest(data[i]);
			}
		};

		self.Delete = function (request) {
			alert(request.Subject() + ' deleted');
		};

		self.AddRequest = function (request) {
			self.requests.unshift(new RequestItemViewModel(request));
		};

	}

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
		var skip = $('#Requests-list li:not(.template)').length;
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
				_drawRequests(data);
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
		$('.request-list .arrow-down').hide();
		$('.request-list .loading-gradient').hide();
	}

	function _moreToLoad() {
		$('.request-list .arrow-down').show();
		$('.request-list .loading-gradient').hide();
	}

	function _removeRequest(requestOrListItem) {
		var listItem;
		if (requestOrListItem.Id)
			listItem = $('#Requests-list li[data-mytime-requestid="' + requestOrListItem.Id + '"]');
		else
			listItem = requestOrListItem;
		listItem
			.animate({
				'height': '0',
				'opacity': '0'
			}, 'fast', function () {
				$(this).remove();
				_loadAPageIfRequired();
				_showMessageIfNoRequests();
			});
	}



	function _drawRequests(requests) {
		for (var i = 0; i < requests.length; i++) {
			var request = requests[i];
			_drawRequest(request);
		}
	}

	function _drawRequest(request) {
		$('#Requests-list').append(_createRequestListItem(request));
	}

	function _drawRequestAtTop(request) {
		var request = _createRequestListItem(request)
			.hide()
			;
		$('#Requests-list')
			.prepend(request)
			;
		request.slideDown();
	}

	function _createRequestListItem(request) {
		var listItem = $('#Requests-list li.template')
			.clone(true)
			.removeClass('template')
			;
		listItem.attr('data-mytime-requestid', request.Id);
		listItem.attr('data-mytime-link', request.Link.href);
		listItem.find('.request-data-subject').text(request.Subject);
		listItem.find('.request-data-date').text(request.Dates);
		listItem.find('.request-data-updatedon').text(request.UpdatedOn);
		listItem.find('.request-data-status').text(request.Status);
		listItem.find('.request-data-text').text(request.Text);

		var connector = listItem.find('.request-connector');
		var deleteButton = listItem.find('.request-delete-button');

		if (!request.IsCreatedByUser) {
			var buttonContainer = listItem.find('.request-delete-button-container');
			buttonContainer.hide();
		}

		if (request.Payload != '') {
			listItem.find('.request-data-type').text(request.Type + ' \u2013 ' + request.Payload);
		} else {
			listItem.find('.request-data-type').text(request.Type);
		}
		connector.connector();

		if (request.Link.Methods.indexOf("DELETE") != -1) {
			deleteButton
				.click(function (event) {
					$(this).prop('disabled', true);
					event.stopPropagation();
					_disconnectAll();
					Teleopti.MyTimeWeb.Request.RequestDetail.HideEditSection();
					_deleteRequest(listItem);
				})
				.removeAttr('disabled', 'disabled')
				;
			listItem.hover(function () {
				deleteButton
					.stop(true, true)
					.fadeToggle();
			});
		} else {
			deleteButton.remove();
		}
		return listItem;
	}

	function _deleteRequest(listItem) {
		var url = listItem.data('mytime-link');
		ajax.Ajax({
			url: url,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "DELETE",
			beforeSend: function () {
				Teleopti.MyTimeWeb.Common.LoadingOverlay.Add(listItem);

			},
			complete: function (jqXHR, textStatus) {
				Teleopti.MyTimeWeb.Common.LoadingOverlay.Remove(listItem);
			},
			success: function (data, textStatus, jqXHR) {
				_removeRequest(listItem);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _initListClick() {
		$('#Requests-list li')
			.click(function () {
				_showRequest($(this));
			}
		);
	}

	function _showRequest(listItem) {
		var url = listItem.attr('data-mytime-link');
		var connector = listItem
			.find('.request-connector')
			;
		var bindListItemClick = function _bindClick() {
			listItem.bind('click', function () {
				_showRequest($(this));
			});
		};
		ajax.Ajax({
			url: url,
			dataType: "json",
			type: 'GET',
			beforeSend: function () {
				listItem.unbind('click');
				_disconnectAllOthers(listItem);
				Teleopti.MyTimeWeb.Request.RequestDetail.FadeEditSection(bindListItemClick);
				connector.connector("connecting");
			},
			success: function (data, textStatus, jqXHR) {
				Teleopti.MyTimeWeb.Request.RequestDetail.ShowRequest(data, listItem.position().top - 30);
				connector.connector("connect");
			}
		});

	}

	function _disconnectAll() {
		$('#Requests-list li:not(.template) .request-connector')
			.connector('disconnect')
		;
	}

	function _disconnectAllOthers(listItem) {
		listItem.siblings()
					.not('.template')
					.data('connected', false)
					.find('.request-connector')
					.connector('disconnect')
					;
	}

	return {
		Init: function (readyForInteractionCallback, completelyLoadedCallback, detailViewModel) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			requestDetailViewModel = detailViewModel;
			_initScrollPaging();
			_initListClick();
			pageViewModel = new RequestPageViewModel(requestDetailViewModel);
			var element = $('#Requests-body-inner')[0];

			if (element) ko.applyBindings(pageViewModel, element);
		},
		AddItemAtTop: function (request) {
			pageViewModel.AddRequest(request);
			_drawRequestAtTop(request);
		},
		RemoveItem: function (request) {
			_removeRequest(request);
		},
		DisconnectAll: function () {
			_disconnectAll();
		}
	};

})(jQuery);
