/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.AsmMessageDetail.js"/>

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
    if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
        Teleopti.MyTimeWeb = {};
    }
}

Teleopti.MyTimeWeb.AsmMessageList = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function asmMessageListViewModel() {
		var self = this;

		self.asmMessageList = ko.observableArray();
	    self.asmMessageColumnList = ko.computed(function() {
	        var list = [];
	        var items = self.asmMessageList();
	        var index = 0;
	        ko.utils.arrayForEach(items, function (i) {
	            if (index % 2 == 0) {
	                list.push({ Items: [i] });
	            } else {
	                list[list.length-1].Items.push(i);
	            }
	            index++;
	        });
	        return list;
	    });
		self.shouldShowMessage = ko.computed(function() {
		    return self.asmMessageList().length == 0;
		});
		self.CreateAsmMessageList = function (dataList) {
			var asmMessageItems = new Array();
			$.each(dataList, function (position, element) {
				asmMessageItems.push(new asmMessageItemViewModel(element));
			});

			self.asmMessageList($.merge(self.asmMessageList(), asmMessageItems));
		};

		self.isBadgeFeatureEnabled = ko.observable(false);
	}

	function asmMessageItemViewModel(item) {
		var self = this;
		self.messageType = ko.observable(item.MessageType);
		self.isBadgeMessage = ko.observable(self.messageType() == 1);
		self.title = ko.observable(item.Title);
		self.message = ko.observable(item.Message);
	    self.errorMessage = ko.observable();
		self.date = ko.observable(item.Date);
		self.sender = ko.observable(item.Sender);
		self.messageId = ko.observable(item.MessageId);
		self.isRead = ko.observable(item.IsRead);
		self.isSelected = ko.observable(false);
		self.allowDialogueReply = ko.observable(item.AllowDialogueReply);
		self.isSending = ko.observable(false);
		self.reply = ko.observable('');
		self.selectedReply = ko.observable();

		self.replyOptions = ko.utils.arrayMap(item.ReplyOptions, function (data) {
			return new replyOptionViewModel(data, self);
		});

		self.dialogueMessages = ko.observableArray(ko.utils.arrayMap(item.DialogueMessages, function (data) {
			return new dialogueMessageViewModel(data);
		}));
		self.isRead.subscribe(function () {
			vm.asmMessageList.remove(self);
		});
		self.confirmReadMessage = function (data, event) {
		    if (self.selectedReply() == undefined && self.replyOptions.length > 0)
		        self.selectedReply(self.replyOptions[0].text());
		    
			self.isSending(true);
			_replyToMessage(self);
		};
		self.userMustSelectReplyOption = ko.computed(function () {
			return self.replyOptions.length > 1;
		});

		self.canConfirm = ko.computed(function () {
			if (self.isSending() || (self.allowDialogueReply() && self.reply().length == 0 && (self.selectedReply() == undefined || self.selectedReply() == 'OK')) || self.selectedReply() == undefined && self.userMustSelectReplyOption()) {
				return false;
	}
			return true;
		});

		self.updateItem = function (itemToUpdate) {
			self.date(itemToUpdate.Date);
			var dialogueMessageArray = new Array();
			$.each(itemToUpdate.DialogueMessages, function (position, element) {
				dialogueMessageArray.push(new dialogueMessageViewModel(element));
			});

			self.dialogueMessages(dialogueMessageArray);
		};

	    self.toggleSelected = function() {
	        self.isSelected(!self.isSelected());
	    };
	}

	var dialogueMessageViewModel = function (dialogueMessage) {
		var self = this;
		self.text = ko.observable(dialogueMessage.Text);
		self.sender = ko.observable(dialogueMessage.Sender);
		self.created = ko.observable(dialogueMessage.Created);
	};

	var replyOptionViewModel = function (replyOption, parent) {
		var self = this;
		self.text = ko.observable(replyOption);
		self.select = function () {
			parent.selectedReply(self.text());
		};
	};

	function _addNewMessageAtTop(messageItem) {
		if (typeof vm !== 'undefined') {
			var result = $.grep(vm.asmMessageList(), function (list) {
				return list.messageId() == messageItem.MessageId;
			});
			if (result.length == 1) {
				var messageIndex = $.inArray(result[0], vm.asmMessageList());
				vm.asmMessageList.splice(messageIndex, 1);
				vm.asmMessageList.unshift(result[0]);
				result[0].updateItem(messageItem);
			} else {
				vm.asmMessageList.unshift(new asmMessageItemViewModel(messageItem));
			}
		}
	};

	function _replyToMessage(messageItem) {
		ajax.Ajax({
			url: "Message/Reply",
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			beforeSend: function () {
				_loading();
			},
			data: JSON.stringify({
				Id: messageItem.messageId(),
				Reply: messageItem.reply(),
				ReplyOption: messageItem.selectedReply()
			}),
			success: function (data, textStatus, jqXHR) {
				messageItem.isRead(true);
				_noMoreToLoad();
				messageItem.isSending(false);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				_noMoreToLoad();
				if (jqXHR.status == 400) {
					alert(jqXHR.responseText);
					var data = $.parseJSON(jqXHR.responseText);
					_displayValidationError(data, messageItem);
					messageItem.isSending(false);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				messageItem.isSending(false);
			}
		});
	}

	function _displayValidationError(data, messageItem) {
		var message = data.Errors.join('</br>');
		messageItem.errorMessage(message || '');
	}

	function _initScrollPaging() {
		vm = new asmMessageListViewModel();
		ko.applyBindings(vm, document.getElementById('.asm-messages'));
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

	function _deleteMessage(messageId) {
		var result = $.grep(vm.asmMessageList(), function (list) {
			return list.messageId() == messageId;
		});
		if (result.length == 1) {
			var messageIndex = $.inArray(result[0], vm.asmMessageList());
			vm.asmMessageList.splice(messageIndex, 1);
		}
	}

	function _loadAPage() {
		var skip = $('.message-list .col-xs-6').length;
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
				vm.CreateAsmMessageList(data);

				if (data.length == 0 || data.length < take) {
					_noMoreToLoad();
				} else {
					_moreToLoad();
				}
			},
			error: function () {
			}
		});

		ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=MyTimeWeb_AgentBadge_28913",
			success: function (data) {
				if (data.IsEnabled) {
					vm.isBadgeFeatureEnabled(true);
				}
			}
		});
	}

	function _hasMoreToLoad() {
		return $('.arrow-down').is(':visible');
	}

	function _loading() {
		$('.arrow-down').hide();
		$('.loading-gradient').show();
	}

	function _noMoreToLoad() {
		$('.arrow-down').hide();
		$('.loading-gradient').hide();
	}

	function _moreToLoad() {
		$('.arrow-down').show();
		$('.loading-gradient').hide();
	}

	return {
		Init: function () {
			_initScrollPaging();
		},
		AddNewMessageAtTop: function (messageItem) {
			_addNewMessageAtTop(messageItem);
		},
		//For testing:
		AddReplyText: function (theReply) {
			$.each(vm.asmMessageList(), function (index, value) {
				value.reply(theReply);
			});
		},
		DeleteMessage: function (messageId) {
			_deleteMessage(messageId);
		}
	};

})(jQuery);
