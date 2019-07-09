/* 
 * 
----------------[ Events ]-------------------- 
 *
 */

// The CHILI Editor calls OnEditorEvent method the parent window of the iFrame
// every time a subscribed event is fired

// To subscribe to an event, you just need to use the AddLister method
editorObject.AddListener("PageAdded");

// Then to get notifed of the event you need to setup a function for window.OnEditorEvent on the parent window of the iFrame
window.OnEditorEvent = function (eventName, objectIdThatRaisedEvent)
{
    switch (eventName)
    {
        case "PageAdded":
            // Do something
            Break;
    }
}






/*
 *
----------------[ Geting Properties and Objects ]--------------------
 *
 */

// GetObject
editorObject.GetObject("document.zoom");







/*
 *
----------------[ Setting Properties ]--------------------
 *
 */
// SetProperty
editorObject.SetProperty("document", "zoom", 80);





/*
 *
----------------[ Excuting Function ]--------------------
 *
 */

// ExcuteFunction
editorObject.ExecuteFunction("document", "Save");