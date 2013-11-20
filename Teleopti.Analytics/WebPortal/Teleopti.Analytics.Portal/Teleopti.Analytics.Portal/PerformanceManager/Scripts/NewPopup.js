document.onclick = checkIfNewReportClicked;

function checkIfNewReportClicked(e) {
    var target = (e && e.target) || (event && event.srcElement);
    var popupDiv = document.getElementById('divNewPopup');
    var linkToTriggerPopup = getServerControl('aNewReport', 'a');
    if (linkToTriggerPopup == null) {
        return;
    }
    if (linkToTriggerPopup.disabled == true)
    {
        return;
    }
    checkParent(target) ? popupDiv.style.display = 'none' : null;
    if (target == linkToTriggerPopup)
    {
        popupDiv.style.display = 'block';
        setReportnameFocus();
    }
    else {
        null;
    }
}

function checkParent(t) {
    while (t.parentNode) {
        if (t == document.getElementById('divNewPopup')) {
            return false;
        }
        t = t.parentNode;
    }
    return true;
}

function hideNewPopup() {
    var obj = document.getElementById('divNewPopup');
    obj.style.display = 'none';
}

function validateNewReport() {
    // Get hold of textbox
    //var textBoxName = getReportNameTextBox();
    var textBoxName = getServerControl('inputTextName', 'input');
    
    if (textBoxName != undefined) {
        // Remove spaces, simple and double quotation marks
        textBoxName.value = textBoxName.value.replace(/\'/g, '');
        textBoxName.value = textBoxName.value.replace(/\"/g, '');
        textBoxName.value = textBoxName.value.replace(/^\s+|\s+$/g, '');

        //alert('#' + textBoxName.value + '#');
        if (textBoxName.value.length == 0) {
            alert('Invalid report name.');
            return false;
        }
        else {
            //alert('valid');
            return true;
        }
    }
    else {
        alert('Could not find the report name text box.');
    }  
    
    return false;
}

//function getReportNameTextBox() {
//    var elementArray = document.getElementsByTagName('input');
//    var i = 0;
//    var textBoxNameToSearchFor = /inputTextName/;
//    var textBoxName;

//    for (i = 0; i < elementArray.length; i++) {
//        var foundPos = elementArray[i].id.search(textBoxNameToSearchFor);
//        if (foundPos != -1) {
//            //alert(elementArray[i].id);
//            textBoxName = document.getElementById(elementArray[i].id);
//            break;
//        }
//    }
//    
//    return textBoxName;
//}

function getServerControl(controlNameToSearchFor, elementType) {
    var elementArray = document.getElementsByTagName(elementType);
    var i = 0;
    var control;

    for (i = 0; i < elementArray.length; i++) {
        var foundPos = elementArray[i].id.indexOf(controlNameToSearchFor);
        if (foundPos != -1) {
            //alert(elementArray[i].id);
            control = document.getElementById(elementArray[i].id);
            break;
        }
    }

    return control;
}

function setReportnameFocus() {
    //var textBoxName = getReportNameTextBox();
    var textBoxName = getServerControl('inputTextName', 'input');

    if (textBoxName != undefined) {
        textBoxName.focus();
        textBoxName.value = '';
    }
}

function keyPressHandler() {
    if (event.keyCode == 13) {
        // Enter key
        event.keyCode = 0;
        var okButton = getServerControl('buttonCreate', 'input');
        okButton.click();
    }
    if (event.keyCode == 27) {
        // Esc key
        event.keyCode = 0;
        var cancelButton = document.getElementById('buttonCancel');
        cancelButton.click();
    }
}