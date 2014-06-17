function onReportFrameRSChange()
{
    if (event.srcElement.readyState == 'complete')
    {
        var reportFrame = getReportIFrame();
        var cw = reportFrame.contentWindow;

        try
        {
            // Once current report’s IFRAME readyState becomes “complete” and its content is no longer Dummy.htm, then we can begin assign delegates.
            if (cw.location.pathname.indexOf('Dummy.htm') == -1)
            {
                // assign different delegates
                //alert('delagates');
                //cw.reportLoadedDelegate = onReportLoaded;
                cw.callbackDelegate = onReportCallback;
                //cw.openDialogDelegate = onDialogOpened;
                //cw.closeDialogDelegate = onDialogClosed;
                //cw.exceptionDelegate = onReportException;
                //cw.expireDelegate = onSessionExpired;
            }
        }
        catch (e)
        {
            //Do nothing. This only happens if Analyzer and Teleopti WFM Web is on different sites.
        }
    }
}

function onReportLoaded()
{
    alert('onReportLoaded()');
}

var frame = null;
function onReportCallback()
{
    if (!frame)
    {
        frame = document.createElement("iframe");
        frame.style.display = 'none';
        frame.src = 'KeepAlive.aspx';
        frame.onreadystatechange = onPosted;

        document.body.appendChild(frame);
    }
}

function onDialogOpened()
{
    alert('onDialogOpened()');
}

function onDialogClosed()
{
    alert('onDialogClosed()');
}

function onReportException()
{
    alert('onReportException()');
}

function onSessionExpired()
{
    alert('onSessionExpired()');
}

function getReportIFrame()
{
    var elementArray = document.getElementsByTagName('iframe');
    var i = 0;
    var controlNameToSearchFor = /AnalyzerFrame/;
    var control;

    for (i = 0; i < elementArray.length; i++)
    {
        var foundPos = elementArray[i].id.search(controlNameToSearchFor);
        if (foundPos != -1)
        {
            //alert(elementArray[i].id);
            control = document.getElementById(elementArray[i].id);
            break;
        }
    }

    return control;
}

function onPosted()
{
    if (frame && frame.readyState == 'complete')
    {
        document.body.removeChild(frame);
        frame = null;
    }
}