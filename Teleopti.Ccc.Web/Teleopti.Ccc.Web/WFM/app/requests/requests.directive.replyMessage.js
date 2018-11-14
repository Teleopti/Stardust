'use strict';
(function() {
	angular
		.module('wfm.requests')
		.controller('requestsReplyMessageController', requestsReplyMessageController)
		.directive('requestsReplyMessage', replyMessageDirective);

	requestsReplyMessageController.$inject = ['requestsPermissions'];

	function requestsReplyMessageController(requestsPermissions) {
		var vm = this;
		vm.replyAndApprove = replyAndApprove;
		vm.replyAndCancel = replyAndCancel;
		vm.replyAndDeny = replyAndDeny;
		vm.replyOnly = replyOnly;
		vm.showReplyAndApproveOrDeny = requestsPermissions.all().HasApproveOrDenyPermission;
		vm.showReplyAndCancel = requestsPermissions.all().HasCancelPermission;

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
			controller: 'requestsReplyMessageController',
			controllerAs: 'requestsReplyMessage',
			templateUrl: 'app/requests/html/requests-reply-message.html',
			link: link
		};

		function link(scope, elem, attrs) {
			scope.$watch('requestsReplyMessage.showReplyDialog', function(newValue, oldValue) {
				if (newValue) {
					var innerScope = angular.element(elem.find('form')).scope();
					var form = innerScope.replyDialogForm;
					form.$setUntouched();
					form.$setPristine();
				}
			});
		}
	}
})();
