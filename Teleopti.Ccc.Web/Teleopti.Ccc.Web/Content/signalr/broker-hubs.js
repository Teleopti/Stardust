﻿/*!
 * ASP.NET SignalR JavaScript Library v2.2.2
 * http://signalr.net/
 *
 * Copyright (c) .NET Foundation. All rights reserved.
 * Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 *
 */

/// <reference path="..\..\SignalR.Client.JS\Scripts\jquery-1.6.4.js" />
/// <reference path="jquery.signalR.js" />
(function ($, window, undefined) {
	/// <param name="$" type="jQuery" />
	"use strict";

	if (typeof ($.signalR) !== "function") {
		throw new Error("SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.");
	}

	var signalR = $.signalR;

	function makeProxyCallback(hub, callback) {
		return function () {
			// Call the client hub method
			callback.apply(hub, $.makeArray(arguments));
		};
	}

	function registerHubProxies(instance, shouldSubscribe) {
		var key, hub, memberKey, memberValue, subscriptionMethod;

		for (key in instance) {
			if (instance.hasOwnProperty(key)) {
				hub = instance[key];

				if (!(hub.hubName)) {
					// Not a client hub
					continue;
				}

				if (shouldSubscribe) {
					// We want to subscribe to the hub events
					subscriptionMethod = hub.on;
				} else {
					// We want to unsubscribe from the hub events
					subscriptionMethod = hub.off;
				}

				// Loop through all members on the hub and find client hub functions to subscribe/unsubscribe
				for (memberKey in hub.client) {
					if (hub.client.hasOwnProperty(memberKey)) {
						memberValue = hub.client[memberKey];

						if (!$.isFunction(memberValue)) {
							// Not a client hub function
							continue;
						}

						subscriptionMethod.call(hub, memberKey, makeProxyCallback(hub, memberValue));
					}
				}
			}
		}
	}

	$.hubConnection.prototype.createHubProxies = function () {
		var proxies = {};
		this.starting(function () {
			// Register the hub proxies as subscribed
			// (instance, shouldSubscribe)
			registerHubProxies(proxies, true);

			this._registerSubscribedHubs();
		}).disconnected(function () {
			// Unsubscribe all hub proxies when we "disconnect".  This is to ensure that we do not re-add functional call backs.
			// (instance, shouldSubscribe)
			registerHubProxies(proxies, false);
		});

		proxies['backportableHub'] = this.createHubProxy('backportableHub');
		proxies['backportableHub'].client = {};
		proxies['backportableHub'].server = {
		};

		proxies['MessageBrokerHub'] = this.createHubProxy('MessageBrokerHub');
		proxies['MessageBrokerHub'].client = {};
		proxies['MessageBrokerHub'].server = {
			addSubscription: function (subscription) {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["AddSubscription"], $.makeArray(arguments)));
			},

			notifyClients: function (message) {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["NotifyClients"], $.makeArray(arguments)));
			},

			notifyClientsMultiple: function (notifications) {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["NotifyClientsMultiple"], $.makeArray(arguments)));
			},

			ping: function () {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["Ping"], $.makeArray(arguments)));
			},

			pingWithId: function (id) {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["PingWithId"], $.makeArray(arguments)));
			},

			removeSubscription: function (route) {
				return proxies['MessageBrokerHub'].invoke.apply(proxies['MessageBrokerHub'], $.merge(["RemoveSubscription"], $.makeArray(arguments)));
			}
		};

		return proxies;
	};

	signalR.hub = $.hubConnection("/signalr", { useDefaultPath: false });
	$.extend(signalR, signalR.hub.createHubProxies());

}(window.jQuery, window));