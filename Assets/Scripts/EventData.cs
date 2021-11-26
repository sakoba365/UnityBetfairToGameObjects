/*
 sako Adam <sakoadam@googlemail.com>
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Class to create Scriptable Object to hold API data
 * Add a submenu to Asset for manual creation
 * 
 */
[CreateAssetMenu(fileName = "Betfair", menuName = "Betfair Event Creation/Data")]
public class EventData : ScriptableObject
{
    /*
     * List of json fields to hold in object data
     */
    public string eventId;
    public string eventName;
    public string marketId;
    public string marketName;
    public string marketStartTime;
    public string eventType;
    public List<string> runners;

    //Print Current Data
    public void PrintMessage()
    {
        Debug.Log("The " + eventName + "  has been loaded.");
    }
    //Placeholder function to update event data
    public void HoldData(string eventId, string eventName)
    {
        this.eventId = eventId;
        this.eventName = eventName;
    }
}
