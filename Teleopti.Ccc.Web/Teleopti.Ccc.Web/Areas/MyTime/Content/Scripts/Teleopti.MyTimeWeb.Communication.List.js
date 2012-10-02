/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="jquery.ui.connector.js"/>

Teleopti.MyTimeWeb.CommunicationList = (function ($) {

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
		var skip = $('#Communications-list li:not(.template)').length;
		var take = 20;
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: "Message/Messages",
			dataType: "json",
			type: 'GET',
			beforeSend: _loading,
			data: {
				Take: take,
				Skip: skip
			},
			success: function (data, textStatus, jqXHR) {
				_drawCommunications(data);
				if (data.length == 0 || data.length < take) {
					_noMoreToLoad();
				} else {
					_moreToLoad();
				}
				_showMessageIfNoCommunications();
			}
		});
	}

	function _hasMoreToLoad() {
		return $('.communication-list .arrow-down').is(':visible');
	}

	function _loading() {
		$('.communication-list .arrow-down').hide();
		$('.communication-list .loading-gradient').show();
	}

	function _noMoreToLoad() {
		$('.communication-list .arrow-down').hide();
		$('.communication-list .loading-gradient').hide();
	}

	function _moreToLoad() {
		$('.communication-list .arrow-down').show();
		$('.communication-list .loading-gradient').hide();
	}

	function _showMessageIfNoCommunications() {
		if ($('.communication-item').not('.template').is(':visible')) {
			$('.friendly-message').hide();
		} else {
			$('.friendly-message').show();
		}
	}

	function _removeCommunication(communicationOrListItem) {
		var listItem;
		if (communicationOrListItem.Id)
			listItem = $('#Communications-list li[data-mytime-communicationid="' + communicationOrListItem.Id + '"]');
		else
			listItem = communicationOrListItem;
		listItem
			.animate({
				'height': '0',
				'opacity': '0'
			}, 'fast', function () {
				$(this).remove();
				_loadAPageIfRequired();
				_showMessageIfNoCommunications();
			});
	}

	function _drawCommunications(communications) {
		for (var i = 0; i < communications.length; i++) {
			var communication = communications[i];
			_drawCommunication(communication);
		}
	}

	function _drawCommunication(communication) {
		$('#Communications-list').append(_createCommunicationListItem(communication));
	}

	function _drawCommunicationAtTop(communication) {
		var communication = _createCommunicationListItem(communication)
			.hide()
			;
		$('#Communications-list')
			.prepend(communication)
			;
		communication.slideDown();
	}

	function _createCommunicationListItem(communication) {
		var listItem = $('#Communications-list li.template')
			.clone(true)
			.removeClass('template')
			;
		listItem.attr('data-mytime-communicationid', communication.Id);
		//		listItem.attr('data-mytime-link', communication.Link.href);
		listItem.find('.communication-data-subject').text(communication.Title);
		//listItem.find('.communication-data-subject').text('_asjkldfh kasjdfh');
		listItem.find('.communication-data-sender').text(communication.Sender);
		listItem.find('.communication-data-updatedon').text(communication.Date);
		listItem.find('.communication-data-text').text(communication.Message);

		var connector = listItem.find('.communication-connector');
		//var deleteButton = listItem.find('.communication-delete-button');

		connector.connector();

		//		if (communication.Link.Methods.indexOf("DELETE") != -1) {
		//			deleteButton
		//				.click(function (event) {
		//					$(this).prop('disabled', true);
		//					event.stopPropagation();
		//					_disconnectAll();
		//					Teleopti.MyTimeWeb.Communication.CommunicationDetail.HideEditSection();
		//					_deleteCommunication(listItem);
		//				})
		//				.removeAttr('disabled', 'disabled')
		//				;
		//			listItem.hover(function () {
		//				deleteButton
		//					.stop(true, true)
		//					.fadeToggle();
		//			});
		//		} else {
		//			deleteButton.remove();
		//		}
		return listItem;
	}

	function _deleteCommunication(listItem) {
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
				_removeCommunication(listItem);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _initListClick() {
		$('#Communications-list li')
			.click(function () {
				_showCommunication($(this));
			}
		);
	}

	function _showCommunication(listItem) {
		var url = listItem.attr('data-mytime-link');
		var connector = listItem
			.find('.communication-connector')
			;
		var bindListItemClick = function _bindClick() {
			listItem.bind('click', function () {
				_showCommunication($(this));
			});
		};
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: url,
			dataType: "json",
			type: 'GET',
			beforeSend: function () {
				listItem.unbind('click');
				_disconnectAllOthers(listItem);
				Teleopti.MyTimeWeb.Communication.CommunicationDetail.FadeEditSection(bindListItemClick);
				connector.connector("connecting");
			},
			success: function (data, textStatus, jqXHR) {
				Teleopti.MyTimeWeb.Communication.CommunicationDetail.ShowCommunication(data, listItem.position().top - 30);
				connector.connector("connect");
			}
		});

	}

	function _disconnectAll() {
		$('#Communications-list li:not(.template) .communication-connector')
			.connector('disconnect')
		;
	}

	function _disconnectAllOthers(listItem) {
		listItem.siblings()
					.not('.template')
					.data('connected', false)
					.find('.communication-connector')
					.connector('disconnect')
					;
	}

	return {
		Init: function () {
			_initScrollPaging();
			_initListClick();
		},
		AddItemAtTop: function (communication) {
			_drawCommunicationAtTop(communication);
			_showMessageIfNoCommunications();
		},
		RemoveItem: function (communication) {
			_removeCommunication(communication);
		},
		DisconnectAll: function () {
			_disconnectAll();
		}
	};

})(jQuery);
