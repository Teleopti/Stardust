/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

Teleopti.MyTimeWeb.CommunicationList = (function ($) {

	var vm;

	function _initScrollPaging() {
		vm = new communicationViewModel();
		ko.applyBindings(vm, document.getElementById('Communication-body-inner'));
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
				vm.communicationList(data);
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

	function communicationViewModel() {
		var self = this;
		self.communicationList = ko.observableArray();
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
		RemoveItem: function (communication) {
			_removeCommunication(communication);
		},
		DisconnectAll: function () {
			_disconnectAll();
		}
	};

})(jQuery);
