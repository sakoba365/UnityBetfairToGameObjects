/*
 sako Adam <sakoadam@googlemail.com>
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Runtime.Serialization.Json;
using System.Linq;

public class ScriptFetch : MonoBehaviour
{
    private string endpoint = "https://api.betfair.com/exchange/betting/json-rpc/v1";//API endpoint
    private string apiKey = ""; //API key provided by Betfair
    private string username = ""; // Betfair username
    private string password = ""; //Betfair password

    // scriptable object
    EventData eventData;

    /*
     * Start is called before the first frame update
     * 
     */
    void Start()
    {
        
        //Create an object of our custom 'Betfair' class
        Betfair betfair = new Betfair(endpoint, apiKey, username, password);
        //Reauest events from specific event type for specific date range
        string from = "2021-10-29T00:00:00Z"; // date from
        string to = "2021-10-31T23:59:00Z"; // date to
        int eventType = 1; // event type - Horse racing -
        // send request to load and save events in a json file
        StartCoroutine(betfair.GetMarketCatalog(eventType, from, to));
        // Create games objects based on saved json hierarchy
        //betfair.CreateGameObjects();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
class Betfair
{
    /*
     * Custom Betfair class to handle all operation
     * 
     */

    // Declare global variables to be used throughout the class
    private string endpoint;
    private string apiKey;
    private string sessionId;
    private string username;
    private string password;
    private string clientCert;
    private string clientCertKey;

    // Define a constructor with API details as parameters
    public Betfair(string endpoint, string apiKey,
        string username = null, string password = null,
        string clientCert=null, string clientCertKey=null)
    {
        // assign received values to the global variables
        this.endpoint = endpoint;
        this.apiKey = apiKey;
        this.username = username;
        this.password = password;
        this.clientCert = clientCert;
        this.clientCertKey = clientCertKey;
    }
   /*
    * This function handles user authentication
    * in betfair
    */
    public IEnumerator Auth()
    {
        // Build the request form fields
        WWWForm form = new WWWForm();
        form.AddField("username", this.username);//set username field
        form.AddField("password", this.password); // set password field
        // Send the request to the login endpoint with the form values
        using (UnityWebRequest www = UnityWebRequest.Post("https://identitysso.betfair.com/api/login", form))
        {
            // Set reauest headers
            www.SetRequestHeader("X-Application", this.apiKey); // set api key
            www.SetRequestHeader("Accept", "application/json"); // set accept type
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded"); // set content type
            // supply the request
            yield return www.SendWebRequest();

            // If the request fails send the error details to log
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.responseCode);
                Debug.Log("Error detail: " + www.error);
                Debug.Log("Error text: " + www.downloadHandler.text);
            }
            else
            {
                //request has succeeded
                Debug.Log("Auth process complete!");
                var jsonResult = www.downloadHandler.text;
                JSONNode jsonNode = SimpleJSON.JSON.Parse(jsonResult);
                // check if request has returned the session token
                if(jsonNode["token"] != null)
                {
                    // update global session id
                    this.sessionId = jsonNode["token"];
                    Debug.Log("Session token has been set!");
                }
                else
                {
                    // login failed, send the response to log
                    Debug.Log("Login failed");
                    Debug.Log(www.downloadHandler.text);

                }
            }
        }
    }
   /*
    * This function handles user logout
    * in betfair
    */
    public IEnumerator Logout()
    {
        // Build the request form fields
        WWWForm form = new WWWForm();
        form.AddField("username", this.username);//set username field
        form.AddField("password", this.password); // set password field
        // Send the request to the login endpoint with the form values
        using (UnityWebRequest www = UnityWebRequest.Post("https://identitysso.betfair.com/api/logout", form))
        {

            // Set reauest headers
            www.SetRequestHeader("X-Application", this.apiKey); // set api key
            www.SetRequestHeader("Accept", "application/json"); // set accept type
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded"); // set content type
            // supply the request
            yield return www.SendWebRequest();

            // If the request fails send the error details to log
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.responseCode);
                Debug.Log("Error detail: " + www.error);
                Debug.Log("Error text: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Logout process complete!");
                Debug.Log(www.downloadHandler.text);

            }
        }
    }

    /*
     * This function handles betfair market catalog
     * @eventTypeIds: filter to this event
     * @from: filter from this date
     * @to: filter to this date
     */
    public IEnumerator GetMarketCatalog(int eventTypeIds, string from, string to)
    {
        /* 
         * Start by authenticating user
         * 
         */
        // Build auth request form fields
        WWWForm formAuth = new WWWForm();
        formAuth.AddField("username", this.username);
        formAuth.AddField("password", this.password);
        // send auth request
        using (UnityWebRequest wwwAuth = UnityWebRequest.Post("https://identitysso.betfair.com/api/login", formAuth))
        {
            //set request header
            wwwAuth.SetRequestHeader("X-Application", this.apiKey); // set api key
            wwwAuth.SetRequestHeader("Accept", "application/json"); // set accept type
            wwwAuth.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded"); // set content type
            // send auth request
            yield return wwwAuth.SendWebRequest();
            // If the request fails send the error details to log
            if (wwwAuth.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + wwwAuth.responseCode);
                Debug.Log("Error detail: " + wwwAuth.error);
                Debug.Log("Error text: " + wwwAuth.downloadHandler.text);
            }
            else
            {
                // reauest succeeded
                Debug.Log("Auth process complete!");
                Debug.Log(wwwAuth.downloadHandler.text);
                var jsonResultAuth = wwwAuth.downloadHandler.text;
                JSONNode jsonNodeAuth = SimpleJSON.JSON.Parse(jsonResultAuth);
                // check if the endpoind has returned the session token
                if (jsonNodeAuth["token"] != null)
                {
                    // if so, then
                    this.sessionId = jsonNodeAuth["token"];
                    Debug.Log("Authe success. Session token has been set!");

                    // Build json rpc parameters
                    var param = "{\"jsonrpc\": \"2.0\", \"method\": \"SportsAPING/v1.0/listMarketCatalogue\", " +
                        "\"params\": {\"filter\":{\"eventTypeIds\":[\"" + eventTypeIds + "\"]," +
                        "\"marketStartTime\":{\"from\":\"" + from + "\", \"to\":\"" + to + "\"} }, " +
                        "\"maxResults\": \"50\", \"marketProjection\": [\"COMPETITION\",\"EVENT\",\"EVENT_TYPE\"," +
                        "\"RUNNER_DESCRIPTION\",\"RUNNER_METADATA\",\"MARKET_START_TIME\"]}," +
                        " \"id\": 1}";
                    // Build request body
                    var www = new UnityWebRequest(this.endpoint, "POST");
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(param);
                    // add json to the body
                    www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw); 
                    www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    // set request header
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.SetRequestHeader("X-Authentication", this.sessionId);
                    www.SetRequestHeader("X-Application", this.apiKey);
                    // send the request to Betfair
                    yield return www.SendWebRequest();
                    // If the request fails, send error detail to log
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Error: " + www.responseCode);
                        Debug.Log("Error detail: " + www.error);
                        Debug.Log("Error text: " + www.downloadHandler.text);
                    }
                    else
                    {
                        Debug.Log("Event request succeed");

                        //Request succeeded
                        var jsonResult = www.downloadHandler.text;
                        Debug.Log(jsonResult);
                        //parse the json date using SimpleJson
                        JSONNode jsonNode = SimpleJSON.JSON.Parse(jsonResult);
                        if (jsonNode["result"] != null)
                        {
                            Debug.Log("Loop through events");
                            // If we already have some event, load them first
                            Dictionary<string, object> event_data =
                                    this.LoadSavedData();
                            foreach (var elem in jsonNode["result"])
                            {
                                Dictionary<string, object> event_dict =
                                    new Dictionary<string, object>();

                                // get event and market details
                                string eventId = elem.Value["event"]["id"];
                                string eventName = elem.Value["event"]["name"];
                                string eventType = elem.Value["eventType"]["name"];
                                string marketId = elem.Value["marketId"];
                                string marketName = elem.Value["marketName"];
                                string marketStartTime = elem.Value["marketStartTime"];

                                event_dict.Add("eventId", eventId);
                                event_dict.Add("eventName", eventName);
                                event_dict.Add("eventType", eventType);
                                event_dict.Add("marketStartTime", marketStartTime);
                                event_dict.Add("marketId", marketId);
                                event_dict.Add("marketName", marketName);

                                ArrayList list_runner = new ArrayList();
                                var all_runners = elem.Value["runners"];
                                foreach (var runner in all_runners)
                                {
                                    string runner_name = runner.Value["runnerName"];
                                    list_runner.Add(runner_name);
                                }
                                event_dict.Add("runners", WriteFromObject(list_runner));
                                /*
                                 * check if event has been saved, update it
                                 * otherwise create it
                                 */
                                if (!event_data.ContainsKey(marketId))
                                {
                                    Debug.Log("Market does not exist, create it - " + marketId);
                                    event_data.Add(marketId, ConvertDictToJson(event_dict));
                                }
                                else
                                {
                                    event_data[marketId] = ConvertDictToJson(event_dict);
                                    Debug.Log("Market exists, update it - " + marketId);
                                }

                            }

                            // tidy up json data
                            string eventDataJson = ConvertDictToJson(event_data);
                            eventDataJson = eventDataJson.Replace("\"{", "{");
                            eventDataJson = eventDataJson.Replace("}\"", "}");
                            eventDataJson = eventDataJson.Replace("\"[", "[");
                            eventDataJson = eventDataJson.Replace("]\"", "]");
                            // save json structure to the file
                            this.WriteData("Assets/Resources/events.json", eventDataJson);
                            this.CreateGameObjects();



                        }
                        else
                        {
                            Debug.Log("Result key not found");
                            Debug.Log(param);
                        }


                    }
                }
            }
        }
    }
    /*
     * Load events json from file
     */
    private Dictionary<string, object> LoadSavedData()
    {
        string path = "Assets/Resources/events.json";// file path
        // read data from file
        string savedJson = RedData(path);
        JSONNode jsonNode = SimpleJSON.JSON.Parse(savedJson);
        Dictionary<string, object> event_data =
                        new Dictionary<string, object>();
        //loop through json structure
        foreach (var key in jsonNode.Keys)
        {
            Dictionary<string, object> event_dict =
                        new Dictionary<string, object>();

            //get elements of the structures
            var elem = jsonNode[key];
            string eventId = elem["eventId"];
            string eventName = elem["eventName"];
            string eventType = elem["eventType"];
            string marketId = elem["marketId"];
            string marketName = elem["marketName"];
            string marketStartTime = elem["marketStartTime"];

            event_dict.Add("eventId", eventId);
            event_dict.Add("eventName", eventName);
            event_dict.Add("eventType", eventType);
            event_dict.Add("marketStartTime", marketStartTime);
            event_dict.Add("marketId", marketId);
            event_dict.Add("marketName", marketName);

            ArrayList list_runner = new ArrayList();
            var all_runners = elem["runners"];
            foreach (var runner in all_runners)
            {
                string runner_name = runner.Value;
                list_runner.Add(runner_name);
            }
            // store elements in a dictionary
            event_dict.Add("runners", WriteFromObject(list_runner));
            event_data.Add(marketId, ConvertDictToJson(event_dict));
        }

        // return the dictionary
        return event_data;
    }
    /*
     * write string to  a file
     * @path: file path
     * @data: text to write
     * 
     */
    private void WriteData(string path, string data)
    {

        //Write data to the file
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(data);
        writer.Close();

    }
  /*
   * load string from  a file
   * @path: file path
   * 
   */
    private string RedData(string path)
    {
        
        StreamReader reader = new StreamReader(path);
        string data = reader.ReadToEnd();
        reader.Close();

        return data;
    }
    /*
     * stream a dictionary object
     * 
     */
    private static string WriteFromObject(object obj)
    {
        byte[] json;
        //Create a stream to serialize the object to.  
        using (MemoryStream ms = new MemoryStream())
        {
            // Serializer the object to the stream.  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(ms, obj);
            json = ms.ToArray();
            ms.Close();
        }
        return Encoding.UTF8.GetString(json, 0, json.Length);

    }
    /*
     * A custom function to convert Dictionary to JSON
     * @dict: dictionary to convert
     */
    string ConvertDictToJson(Dictionary<string, object> dict)
    {
        var entries = dict.Select(d =>
            string.Format("\"{0}\": {1}", d.Key, string.Join(",", "\""+d.Value+"\"")));
        return "{" + string.Join(",", entries) + "}";
    }
    /*
     * Create game objects from the saved
     * json structure
     */
    public void CreateGameObjects()
    {
        string path = "Assets/Resources/events.json";// json file path
        // read data in string
        string savedJson = RedData(path);
        // convert string to json object using SimpleJson
        JSONNode jsonNode = SimpleJSON.JSON.Parse(savedJson);

        Dictionary<string, GameObject> eventObjects =
                        new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> eventTypeObjects =
                new Dictionary<string, GameObject>();
        //loop through json structure
        foreach (var key in jsonNode.Keys)
        {
            // get json elements
            var elem = jsonNode[key];
            string eventId = elem["eventId"];
            string eventName = elem["eventName"];
            string eventType = elem["eventType"];
            string marketId = elem["marketId"];
            string marketName = elem["marketName"];
            string marketStartTime = elem["marketStartTime"];
            var runners = elem["runners"];

            Debug.Log("eventType: " + eventType);
            Debug.Log("Creating Game object - " + eventName);

            //Check if event type is in root
            if (!eventTypeObjects.ContainsKey(eventType))
            {
                GameObject eventTypeToSpawn = new GameObject(eventType);

                // check if game opbject exists in root
                if (!eventObjects.ContainsKey(eventName))
                {

                    //If no, then create it with children game objetcs
                    GameObject eventToSpawn = new GameObject(eventName);
                    eventToSpawn.transform.parent = eventTypeToSpawn.gameObject.transform;
                    // attach a script to each event with the json data
                    EventLoader eventData = eventToSpawn.AddComponent<EventLoader>();
                    eventData.eventId = eventId;
                    eventData.eventName = eventName;
                    eventData.marketId = marketId;
                    eventData.marketName = marketName;
                    eventData.marketStartTime = marketStartTime;
                    eventData.eventType = eventType;
                    eventData.runners = runners;
                    

                    GameObject marketToSpawnChil = new GameObject(marketName);
                    marketToSpawnChil.transform.parent = eventToSpawn.gameObject.transform;
                    this.AttachScript(marketToSpawnChil, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    foreach (var runner in runners)
                    {
                        GameObject runnerToSpawn = new GameObject(runner.Value);
                        runnerToSpawn.transform.parent = marketToSpawnChil.gameObject.transform;
                        this.AttachScript(runnerToSpawn, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                           runners);
                    }

                    eventObjects.Add(eventName, eventToSpawn);
                }
                else
                {
                    // if so, then append the child to the root
                    GameObject eventToSpawn = eventObjects[eventName];
                    GameObject marketToSpawnChil = new GameObject(marketName);
                    marketToSpawnChil.transform.parent = eventToSpawn.gameObject.transform;
                    this.AttachScript(marketToSpawnChil, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    foreach (var runner in runners)
                    {
                        GameObject runnerToSpawn = new GameObject(runner.Value);
                        runnerToSpawn.transform.parent = marketToSpawnChil.gameObject.transform;
                        this.AttachScript(runnerToSpawn, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    }
                }
                eventTypeObjects.Add(eventType, eventTypeToSpawn);
            }
            else
            {
                GameObject eventTypeToSpawn = eventTypeObjects[eventType];

                // check if game opbject exists in root
                if (!eventObjects.ContainsKey(eventName))
                {

                    //If no, then create it with children game objetcs
                    GameObject eventToSpawn = new GameObject(eventName);
                    eventToSpawn.transform.parent = eventTypeToSpawn.gameObject.transform;

                    // attach a script to each event with the json data
                    EventLoader eventData = eventToSpawn.AddComponent<EventLoader>();
                    eventData.eventId = eventId;
                    eventData.eventName = eventName;
                    eventData.marketId = marketId;
                    eventData.marketName = marketName;
                    eventData.marketStartTime = marketStartTime;
                    eventData.eventType = eventType;
                    eventData.runners = runners;

                    GameObject marketToSpawnChil = new GameObject(marketName);
                    marketToSpawnChil.transform.parent = eventToSpawn.gameObject.transform;
                    this.AttachScript(marketToSpawnChil, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    foreach (var runner in runners)
                    {
                        GameObject runnerToSpawn = new GameObject(runner.Value);
                        runnerToSpawn.transform.parent = marketToSpawnChil.gameObject.transform;
                        this.AttachScript(runnerToSpawn, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    }

                    eventObjects.Add(eventName, eventToSpawn);
                }
                else
                {
                    // if so, then append the child to the root
                    GameObject eventToSpawn = eventObjects[eventName];
                    GameObject marketToSpawnChil = new GameObject(marketName);
                    marketToSpawnChil.transform.parent = eventToSpawn.gameObject.transform;
                    this.AttachScript(marketToSpawnChil, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    foreach (var runner in runners)
                    {
                        GameObject runnerToSpawn = new GameObject(runner.Value);
                        runnerToSpawn.transform.parent = marketToSpawnChil.gameObject.transform;
                        this.AttachScript(runnerToSpawn, eventId, eventName, marketId, marketName, marketStartTime, eventType,
                            runners);
                    }
                }
            }

            Debug.Log("Game object created - "+ eventName);

        }

    }
    // Attach script to a game object
    public void AttachScript(GameObject marketObject, string eventId, string eventName, string marketId, string marketName, string marketStartTime, string eventType, 
                            List<string> runners)
    {
        EventLoader eventData = marketObject.AddComponent<EventLoader>();
        eventData.eventId = eventId;
        eventData.eventName = eventName;
        eventData.marketId = marketId;
        eventData.marketName = marketName;
        eventData.marketStartTime = marketStartTime;
        eventData.eventType = eventType;
        eventData.runners = runners;
    }
}

