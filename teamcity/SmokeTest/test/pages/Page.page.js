function Page () {
    this.title = 'My Page';
	this.webUrl = process.env.UrlToTest + '/Web';
}

Page.prototype.open = function (path) {
	console.log('navigate to url ' + this.webUrl + '/' + path);
    browser.url(this.webUrl + '/' + path)
}

module.exports = new Page()