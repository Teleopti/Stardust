var webdriverio = require('webdriverio');
var request = require("request");
var client = webdriverio
	.remote({
		desiredCapabilities: {
			browserName: 'chrome'
		}
	});
	
var log = function(msg){
	if(client)
		client.call(function(){console.log(msg)});
};

var closeAndThrow = function(msg){
	log(msg);
	log('shutdown client.');
	client.end(function(){
		log('shutdown selenium server');
		request('http://localhost:4444/selenium-server/driver/?cmd=shutDownSeleniumServer',function (error, response, body) {
			throw new Error(msg);
		});
	});
};	

client.init();
var webUrl = process.env.UrlToTest + '/Web';
log('navigate to url ' + webUrl);
client.url(webUrl)
	.waitForExist('#Username-input', 60000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('failed to navigate to sign in page. ' + err);
		}
	});
log('try to sign in');
client.setValue('#Username-input', 'demo')
	.setValue('#Password-input', 'demo')
	.click('#Signin-button')
	.waitForExist('.user-name', 120000, false, function(err, res, response) {
		if (err || !res) {
			log('failed to sign in first time. res: ' + res);
			client.pause(5000);
			client.url(webUrl)
				.waitForExist('.user-name', 120000, false, function(err, res, response) {
					if (err || !res) {
						log('failed to sign in second time. res: ' + res);
						closeAndThrow('failed to sign in. ' + err);
					}else{
						log('sign in succeeded second time:' + res);
					}
				});
		}else{
			log('sign in succeeded first time:' + res);
		}
	});

log('navigate to health check');
client.url(webUrl + '/HealthCheck')
	.waitForExist(".services li span", 600000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('service bus isnot up and running after trying 10 minutes. ' + err);
		}
	});
log('check service bus and broker');
client.click('#Start-Check');
// have no idea why first time of checking is not working, have to refresh and check it again
client.pause(5000);
client.refresh()
	.waitForExist(".services li span", 600000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('service bus isnot up and running after trying 10 minutes. ' + err);
		}
	});
client.pause(5000);
client.click('#Start-Check')
	.waitForExist('#Bus-Results', 600000, false, function(err, res, response) {
		if (err || !res) {
			closeAndThrow('service bus doesnot work well after trying 10 minutes. ' + err);
		}else{
			log('service bus and broker work well');
		}
	});
	
log('shutdown client.');
client.end();
log('shutdown selenium server');
client.call(function(){
	request('http://localhost:4444/selenium-server/driver/?cmd=shutDownSeleniumServer');
});


