﻿'use strict';

describe('requestsRepltMessagedirectiveTest', function () {
	var $compile,
        $rootScope,
        expectedMessage;
	var cb = function (message) {
		expectedMessage = message;
	}
	beforeEach(function () {
		module('wfm.templates');
		module('wfm.requests');
	});

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
	}));

	it('replyAndApprove, should pass through the message', function () {
		var test = setUpTarget(cb);
		test.targetScope.replyMessage = 'message for replyAndApprove';
		expect(expectedMessage).toEqual('');

		test.targetScope.replyAndApprove();

		expect(expectedMessage).toEqual('message for replyAndApprove');
	});

	it('replyAndDeny, should pass through the message', function () {
		var test = setUpTarget(cb);
		test.targetScope.replyMessage = 'message for replyAndDeny';
		expect(expectedMessage).toEqual('');

		test.targetScope.replyAndDeny();

		expect(expectedMessage).toEqual('message for replyAndDeny');
	});

	it('replyAndCancel, should pass through the message', function () {
		var test = setUpTarget(cb);
		test.targetScope.replyMessage = 'message for replyAndCancel';
		expect(expectedMessage).toEqual('');

		test.targetScope.replyAndCancel();

		expect(expectedMessage).toEqual('message for replyAndCancel');
	});

	it('replyOnly, should pass through the message', function () {
		var test = setUpTarget(cb);
		test.targetScope.replyMessage = 'message for replyOnly';
		expect(expectedMessage).toEqual('');

		test.targetScope.replyOnly();

		expect(expectedMessage).toEqual('message for replyOnly');
	});

	it('should clean up after each operation', function () {
		var test = setUpTarget(cb);

		test.targetScope.replyMessage = 'for clean up';
		test.targetScope.replyAndApprove();
		expect(test.targetScope.replyMessage).toEqual('');

		test.targetScope.replyMessage = 'for clean up';
		test.targetScope.replyAndDeny();
		expect(test.targetScope.replyMessage).toEqual('');

		test.targetScope.replyMessage = 'for clean up';
		test.targetScope.replyAndCancel();
		expect(test.targetScope.replyMessage).toEqual('');

		test.targetScope.replyMessage = 'for clean up';
		test.targetScope.replyOnly();
		expect(test.targetScope.replyMessage).toEqual('');
	});

	function setUpTarget(cb) {
		var rootScope = $rootScope.$new();
		rootScope.requestsCommandsPane = {};
		expectedMessage = '';
		var callback = function (message) {
			cb(message);
		};
		rootScope.requestsCommandsPane.approveRequests = callback;
		rootScope.requestsCommandsPane.denyRequests = callback;
		rootScope.requestsCommandsPane.cancelRequests = callback;
		rootScope.requestsCommandsPane.replyRequests = callback;

		var targetElement = $compile('<requests-reply-message' +
        ' approve="requestsCommandsPane.approveRequests(message)"' +
        ' deny="requestsCommandsPane.denyRequests(message)"' +
        ' cancel="requestsCommandsPane.cancelRequests(message)"' +
        ' reply="requestsCommandsPane.replyRequests(message)"' +
        ' original-message="requestsCommandsPane.selectedRequestMessage"' +
        'show-reply-dialog="requestsCommandsPane.showReplyDialog"></requests-reply-message>')(rootScope);
		rootScope.$digest();
		var target = targetElement.isolateScope();

		return {
			targetScope: target.requestsReplyMessage,
			targetElem: targetElement
		}
	}
});