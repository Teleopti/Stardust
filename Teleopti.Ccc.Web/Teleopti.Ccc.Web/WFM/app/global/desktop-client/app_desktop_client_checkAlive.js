function ahAhAhAhStayingAlive() {
  var request = new XMLHttpRequest();
  var errorreport = function(xhr, ajaxOptions, thrownError) {
        console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        errorStayingAlive();
      };
  request.open('GET', '../api/Global/User/CurrentUser', true);
  request.onload = function() {
    if (request.status >= 200 && request.status < 400) {
      var data = JSON.parse(request.responseText);
      return true;
    } else {errorreport();
    }
  };
  request.send();
}

window.onerror = function(errorMessage, url, line) {
  var req = new XMLHttpRequest();
  req.open('POST', '../api/Logging/LogError', true);
  req.setRequestHeader("Content-type", "application/json");
  req.send(JSON.stringify({
    Message: errorMessage,
    Url: url,
    LineNumber: line,
    ParentUrl: document.location.href,
    UserAgent: navigator.userAgent
  }));
};
