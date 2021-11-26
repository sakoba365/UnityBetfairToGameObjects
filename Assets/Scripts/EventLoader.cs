/*
 sako Adam <sakoadam@googlemail.com>
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Class to load current Data in Object Data
 * 
 */
public class EventLoader : MonoBehaviour
{
    EventData eventData;
    public string eventId;
    public string eventName;
    public string marketId;
    public string marketName;
    public string marketStartTime;
    public string eventType;
    public List<string> runners;
    // print the detail of data attached to the script
    void Start()
    {
       // eventData.PrintMessage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
