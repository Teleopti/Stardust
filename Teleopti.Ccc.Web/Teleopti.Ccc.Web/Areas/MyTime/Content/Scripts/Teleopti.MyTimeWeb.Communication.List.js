/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
    if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
        Teleopti.MyTimeWeb = {};
    }
}

Teleopti.MyTimeWeb.CommunicationList = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function communicationListViewModel() {
		var self = this;

		self.communicationList = ko.observableArray();
		self.chosenMessage = ko.observable();
		self.chosenMessageId = ko.observable();
		self.shouldShowMessage = ko.observable(false);

		self.CreateCommunicationList = function (dataList) {
			//console.log('create list before');
			var communicationItems = new Array();
			$.each(dataList, function (position, element) {
				communicationItems.push(new communicationItemViewModel(element));
			});

			self.communicationList($.merge(self.communicationList(), communicationItems));

			//console.log(self.communicationList());
			//console.log('create list after');
		};

		self.chosenMessageId.subscribe(function () {
			var match = ko.utils.arrayFirst(self.communicationList(), function (item) {
				if (item.messageId() === self.chosenMessageId()) {
					return item;
				}
			});
			self.chosenMessage(match);
			//console.log('Title of chosen: ' + self.chosenMessage().title());
		});

		self.chosenMessage.subscribe(function () {
			if (self.chosenMessage() == null) {
				Teleopti.MyTimeWeb.CommunicationDetail.HideEditSection();
			}
		});

		self.communicationList.subscribe(function () {
			if (self.communicationList().length == 0)
				self.shouldShowMessage(true);
			else {
				self.shouldShowMessage(false);
			}
		});
	}

	function communicationItemViewModel(item) {
		var self = this;
		self.title = ko.observable(item.Title);
		self.message = ko.observable(item.Message);
		self.date = ko.observable(item.Date);
		self.sender = ko.observable(item.Sender);
		self.messageId = ko.observable(item.MessageId);
		self.isRead = ko.observable(item.IsRead);
		self.isConfirmButtonEnabled = ko.observable(true);
		self.confirmReadMessage = function (data, event) {
			//alert("you clicked " + event.target.id);
			//alert("you clicked " + self.messageId());
			_replyToMessage(self);
		};
		self.isRead.subscribe(function () {
			vm.communicationList.remove(self);
			vm.chosenMessage(null);
		});
	}

	function _addNewMessageAtTop(messageItem) {
		vm.communicationList.unshift(new communicationItemViewModel(messageItem));
		setConnectorOnAllConnectionItems();
	};

	function _replyToMessage(messageItem) {
		//alert(messageItem.messageId());
		ajax.Ajax({
			url: "Message/Reply",
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			beforeSend: function () {
				_setConfirmButtonState(messageItem, false);
				_loading();
			},
			data: JSON.stringify({
				MessageId: messageItem.messageId()
			}),
			success: function (data, textStatus, jqXHR) {
				// Ta bort meddelandet mha messageId från vår vm
				messageItem.isRead(true);
				_noMoreToLoad();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				//alert('felfelfel!');
			}
		});
	}

	function _setConfirmButtonState(messageItem, isEnabled) {
		messageItem.isConfirmButtonEnabled(isEnabled);
	}

	function _initScrollPaging() {
		vm = new communicationListViewModel();
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
		ajax.Ajax({
			url: "Message/Messages",
			dataType: "json",
			type: 'GET',
			beforeSend: _loading,
			data: {
				Take: take,
				Skip: skip
			},
			success: function (data, textStatus, jqXHR) {
				//console.log('success');
				vm.CreateCommunicationList(data);

				if (data.length == 0 || data.length < take) {
					_noMoreToLoad();
				} else {
					_moreToLoad();
				}
				_initListClick();
				setConnectorOnAllConnectionItems();
			},
			error: function () {
				//console.log('error');
			}
		});
	}

	function setConnectorOnAllConnectionItems() {
		//loopa igenom listan och sätta connector.connect på alla items
		$('.communication-list li .communication-connector')
			.not('ui-connector')
			.connector();
		//var connector = listItem.find('.communication-connector');
		//connector.connector();
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

	function _initListClick() {
		$('#Communications-list li')
			.unbind('click')
            .click(function () {
            	//console.log('chosen id ' + vm.chosenMessageId());
            	vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
            	_showCommunication($(this));
            }
		);
	}

	function _showCommunication(listItem) {
		//        var url = listItem.attr('data-mytime-link');
		var connector = listItem
			.find('.communication-connector')
			;
		var bindListItemClick = function _bindClick() {
			listItem.bind('click', function () {
				vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
				_showCommunication($(this));
			});
		};

		listItem.unbind('click');
		_disconnectAllOthers(listItem);
		Teleopti.MyTimeWeb.CommunicationDetail.FadeEditSection(bindListItemClick);
		connector.connector("connecting");
		Teleopti.MyTimeWeb.CommunicationDetail.ShowCommunication(listItem.position().top - 30);
		connector.connector("connect");
	}

	function _disconnectAll() {
		$('#Communications-list li:not(.template) .communication-connector')
			.connector('disconnect')
		;
	}

	function _disconnectAllOthers(listItem) {
		listItem.siblings()
					.data('connected', false)
					.find('.communication-connector')
					.connector('disconnect')
					;
	}

	return {
		Init: function () {
			_initScrollPaging();
		},
		DisconnectAll: function () {
			_disconnectAll();
		},
		AddNewMessageAtTop: function (messageItem) {
			_addNewMessageAtTop(messageItem);
		}
	};

})(jQuery);
