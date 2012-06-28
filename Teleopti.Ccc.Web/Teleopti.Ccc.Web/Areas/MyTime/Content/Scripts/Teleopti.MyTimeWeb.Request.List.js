/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>

Teleopti.MyTimeWeb.Request.List = (function ($) {

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
		Teleopti.MyTimeWeb.Ajax.Ajax({
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
				if (data.length == 0 || data.length < take) {
					_noMoreToLoad();
				} else {
					_moreToLoad();
				}
				_showMessageIfNoRequests();
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

	function _showMessageIfNoRequests() {
		if ($('.request-item').not('.template').is(':visible')) {
			$('.friendly-message').hide();
		} else {
			$('.friendly-message').show();
		}
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

		if (request.Payload != '') {
			listItem.find('.request-data-type').text(request.Type + ' \u2013 ' + request.Payload);
		} else {
			listItem.find('.request-data-type').text(request.Type);
		}
		connector.connector();

		var count = 0;
		if (request.Link.Methods.indexOf("DELETE") != -1) {
			deleteButton
				.click(function (event) {
					count++;
					if (count > 1) { return false; }
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
		Teleopti.MyTimeWeb.Ajax.Ajax({
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
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: url,
			dataType: "json",
			type: 'GET',
			beforeSend: function() {
				_disconnectAllOthers(listItem);
				Teleopti.MyTimeWeb.Request.RequestDetail.FadeEditSection();
				connector.connector("connecting");
			},
			success: function(data, textStatus, jqXHR) {
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
		Init: function () {
			_initScrollPaging();
			_initListClick();
		},
		AddItemAtTop: function (request) {
			_drawRequestAtTop(request);
			_showMessageIfNoRequests();
		},
		RemoveItem: function (request) {
			_removeRequest(request);
		},
		DisconnectAll: function () {
			_disconnectAll();
		}
	};

})(jQuery);
