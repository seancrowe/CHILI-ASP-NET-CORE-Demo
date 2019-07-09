var editorObject = null;

window.OnEditorEvent = OnEditorEvent;

window.initialViewRendered=false;
/**
 * Function is called automatically when an event is fired by the editor (this call is done in main.js of the CHILI editor)
 */
function OnEditorEvent(type, targetID) {

    switch (type) {

        // Fired when the document is fully deserialized (serialized) the XML to a JavaScript object
        // This is automatically fired
        case "DocumentFullyLoaded":
            OnEditorLoaded();
            console.log("Document Fully Loaded");
            break;

        // Fired when all frames of the initially visible pages are initialised and rendered
        // Lastest event to be fired 
        // You must add listender for this event
        case "DocumentInitialViewRendered":
            console.log("Initial View Rendered");
            window.initialViewRendered = true;
            break;

        case "ZoomAfterChange":
            UpdateCustomZoom();
            break;

        case "VariableValueChanged":
            UpdateVariableBoxes();
            break;
    }
}

/**
 * Function called when DocumentFullyLoaded is completed
 */
function OnEditorLoaded() {
    window.editorObject = document.getElementById("editor-iframe").contentWindow.editorObject;

    // Add listeners here:
    editorObject.AddListener("DocumentInitialViewRendered");
    editorObject.AddListener("ZoomAfterChange");
    editorObject.AddListener("VariableValueChanged");

    // Run function after console is empty
    // Console is empty when all frames have been loaded
    // Replace null with your function
    CheckReadyForConsole(null);
}

/**
 * Function waits for console to be empty and then it calls the endFunction
 */
function CheckReadyForConsole(endFunction) {
    if (window.editorObject.GetObject("document.readyForConsole") != "") {
        setTimeout(function () { CheckReadyForConsole(endFunction); }, 500);

    }
    else {
        if (endFunction != null) {
            endFunction();
        }
    }
}

/*document.getElementById("js-save").addEventListener('click', function () {
    editorObject.ExecuteFunction("document", "Save");
});*/

document.getElementById("js-save").addEventListener('click', function () {

    if (initialViewRendered == true)
    {
        let documentXml = editorObject.ExecuteFunction("document", "GetTempXML");

        let data = { "documentXml": documentXml };

        // XML upload
        let xmlreq = new XMLHttpRequest();

        xmlreq.open("POST", "/database/save");
        xmlreq.setRequestHeader('Content-Type', 'application/json');
        xmlreq.send(JSON.stringify(data));

        xmlreq.onload = function () {
            let response = xmlreq.responseText;
            let status = xmlreq.status;

            try {
                let result = JSON.parse(response);
                alert("Document Saved");
            }
            catch (e) {

            }
        }
    }
});

document.getElementById("js-zoomValue").addEventListener("keyup", function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        document.getElementById("js-setZoom").click();
    }
});

document.getElementById("js-setZoom").addEventListener('click', function () {
    let zoomValue = document.getElementById("js-zoomValue").value;

    let zoomNumber = parseInt(zoomValue);

    if (zoomNumber < 10)
    {
        zoomNumber = 10;
        document.getElementById("js-zoomValue").value = 10;
    }

    if (zoomNumber > 200)
    {
        zoomNumber = 200;
        
    }

    editorObject.SetProperty("document", "zoom", zoomNumber);
});

function UpdateCustomZoom()
{
    let zoomValue = editorObject.GetObject("document.zoom");
    document.getElementById("js-zoomValue").value = zoomValue;
}


document.getElementById("js-zindexUp").addEventListener("click", function ()
{
    // Get selectedFrame
    var selectedFrame = editorObject.GetObject("document.selectedFrame");

    // If frame is null, then we must have a selected text
    if (selectedFrame == null) {
        selectedFrame = editorObject.GetObject("document.selectedText.frame");
    }

    if (selectedFrame != null) {
        let index = parseInt(editorObject.GetObject("document.selectedFrame.index")) + 1;
        editorObject.SetProperty("document.selectedFrame", "index", index);
    }
});

document.getElementById("js-zindexDown").addEventListener("click", function ()
{
    // Get selectedFrame
    var selectedFrame = editorObject.GetObject("document.selectedFrame");

    // If frame is null, then we must have a selected text
    if (selectedFrame == null) {
        selectedFrame = editorObject.GetObject("document.selectedText.frame");
    }

    if (selectedFrame != null) {
        let index = parseInt(editorObject.GetObject("document.selectedFrame.index")) - 1;
        editorObject.SetProperty("document.selectedFrame", "index", index);
    }
});



document.getElementById("js-updateVariables").addEventListener("click", function ()
{
    let headerValue = document.getElementById("js-header").value
    let subheaderValue = document.getElementById("js-subheader").value

    editorObject.SetProperty("document.variables[Front_Header]", "value", headerValue);
    editorObject.SetProperty("document.variables[Front_SubHeader]", "value", subheaderValue);
});

function UpdateVariableBoxes()
{
    document.getElementById("js-header").value = editorObject.GetObject("document.variables[Front_Header].value");
    document.getElementById("js-subheader").value = editorObject.GetObject("document.variables[Front_SubHeader].value");
}

//DefinitionXML
//imgXML



//editor.ExecuteFunction("document.pages[" + i + "].frames[" + j + "]", "FlattenVariables");



function KeepHelpLayersOnTop()
{
    let layerCount = editorObject.GetObject("document.layers.count");

    let layersOnTop = [];

    for (let i = 0; i < layerCount; i++) {
    
        let layer = editorObject.GetObject("document.layers[" + i + "]");

        if (layer.isHelpLayer == "true")
        {
            layersOnTop.push(layer.id);
        }
    }



    let pageCount = editorObject.GetObject("document.pages");

    let framesOnTop = [];

    for (let i = 0; i < pageCount; i++) {

        let frameCount = editorObject.GetObject("document.pages["+i+"]");

        let framesOnTop = [];

        for (let j = 0; j < frameCount; j++) {
            let frameLayerId = editorObject.GetObject("document.allFrames[" + i + "].layer.id");

            if (layersOnTop.include(frameLayerId) == true) {
                let frame = editorObject.GetObject("document.allFrames[" + i + "]");

                framesOnTop.push(frame);
            }
        }

        if (framesOnTop.length > 0)
        {
            framesOnTop.sort(function (a, b) {
                if (a.index > b.index) return 1;

                return -1;
            });
        }

        while (framesOnTop.length > 0)
        {
            let id = framesOnTop.pop().id;

        }

    }
}

/*let frameCount = editorObject.GetObject("document.allFrames.count");

for (var i = 0; i < frameCount.length; i++) {

    let frameIndex =   editorObject.GetObject("document.allFrames["+i+"].index");

    let xml = '<TextFlow xmlns="http://ns.adobe.com/textLayout/2008"><p fontFamily="Arial_Regular" paragraphStartIndent="0" textIndent="0"><span>'+ frameIndex +'</span></p></TextFlow>';


    editorObject.ExecuteFunction("document.allFrames["+i+"]", "ImportTextFlow", xml);
}
*/