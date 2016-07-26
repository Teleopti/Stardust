'use strict';
(function () {
	angular.module('wfm.requests')
        .controller('requestsReplyMessageCtrl', [requestsReplyMessageController])
        .directive('requestsReplyMessage', replyMessageDirective);

	function requestsReplyMessageController() {
		var vm = this;
		vm.replyAndApprove = replyAndApprove;
		vm.replyAndCancel = replyAndCancel;
		vm.replyAndDeny = replyAndDeny;
		vm.replyOnly = replyOnly;

		function replyAndApprove() {
			vm.approve({ message: vm.replyMessage });
			cleanUp();
		}
		function replyAndDeny() {
			vm.deny({ message: vm.replyMessage });
			cleanUp();

		}
		function replyAndCancel() {
			vm.cancel({ message: vm.replyMessage });
			cleanUp();
		}

		function replyOnly() {
			vm.reply({ message: vm.replyMessage });
			cleanUp();
		}

		function cleanUp() {
			vm.replyMessage = '';
			vm.originalMessage = '';
			vm.showReplyDialog = false;
		}
	}

	function replyMessageDirective() {
		return {
			scope: {
				approve: '&?',
				deny: '&?',
				cancel: '&?',
				reply: '&?',
				showReplyDialog: '=',
				originalMessage: '=',
				showOriginalMessage: '='
			},
			bindToController: true,
			restrict: 'E',
			controller: 'requestsReplyMessageCtrl',
			controllerAs: 'requestsReplyMessage',
			templateUrl: 'js/requests/html/requests-reply-message.html'
		};
	}
}());