using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


using System.Text;
using System.Linq;
using Newtonsoft.Json;
using TMPro;

public class GoveeController : MonoBehaviour
{
    
    string apiKey = "APIKEY";
    string DEVICE_MAC_ADDRESS = "DEV_MAC";
    string DEVICEID = "DEVID";
    string DEVICEMODEL = "DEVMODEL";

    [HideInInspector] public int r; 
    [HideInInspector] public int g; 
    [HideInInspector] public int b; 

    [HideInInspector] public bool power; 

    string queryURL = "https://openapi.api.govee.com/router/api/v1/device/control";

    [System.Serializable]
    public class Capability{

        public string type;
        public string instance;
        public int value;
    }

    [System.Serializable]
    public class Payload{

        public string sku;
        public string device;
        public Capability capability;


    }

    [System.Serializable]
    public class GoveeRequest{

        public string requestId;
        public Payload payload;

    }

    string CreatePowerRequestJson(bool val){

        GoveeRequest gr = new GoveeRequest();
        gr.requestId = "uuid";
        gr.payload = new Payload();
        gr.payload.sku = DEVICEMODEL;
        gr.payload.device = DEVICEID;
        gr.payload.capability = new Capability();
        gr.payload.capability.type = "devices.capabilities.on_off";
        gr.payload.capability.instance = "powerSwitch";
        gr.payload.capability.value = (val) ? 1 : 0;

        return JsonConvert.SerializeObject(gr);

    }

    string CreateColorRequestJson(int r, int g, int b){

        GoveeRequest gr = new GoveeRequest();
        gr.requestId = "uuid";
        gr.payload = new Payload();
        gr.payload.sku = DEVICEMODEL;
        gr.payload.device = DEVICEID;
        gr.payload.capability = new Capability();
        gr.payload.capability.type = "devices.capabilities.color_setting";
        gr.payload.capability.instance = "colorRgb";
        gr.payload.capability.value = ((r&0xFF)<<16) | ((g&0xFF)<<8) | ((b&0xFF) << 0);

        return JsonConvert.SerializeObject(gr);

    }

    public void RunPowerQueryGoveeHFL(bool v){

        string jsonBody = CreatePowerRequestJson(v);

        StartCoroutine(HTTPostHFL(queryURL,
                                  jsonBody, 
                                  (msg) => Debug.Log("Govee Query success: " + msg), //success 
                                  (emsg) => Debug.Log("Govee Query Failed: " + emsg))); //fail

    }

    public void RunColorQueryGoveeHFL(int r, int g, int b){

        string jsonBody = CreateColorRequestJson(r,g,b);

        StartCoroutine(HTTPostHFL(queryURL,
                                  jsonBody, 
                                  (msg) => Debug.Log("Govee Query success: " + msg), //success 
                                  (emsg) => Debug.Log("Govee Query Failed: " + emsg))); //fail

    }


    public IEnumerator HTTPostHFL(string url, string bodyJsonString, Action<string> successfn, Action<string> failurefn){

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);

        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Govee-API-Key", apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            failurefn(request.error);
        }
        else
        {
        // Extract and log the text content from the first candidate's first part
            successfn(request.downloadHandler.text);
        }

    }

    public void ColorSliderRValChange(float nv){
        r = (int)nv;
        RunColorQueryGoveeHFL(r,g,b);
    }

    public void ColorSliderGValChange(float nv){
        g = (int)nv;
        RunColorQueryGoveeHFL(r,g,b);
    }

    public void ColorSliderBValChange(float nv){
        b = (int)nv;
        RunColorQueryGoveeHFL(r,g,b);
    }

    public void PowerButtonPress(){
        power = !power;
        RunPowerQueryGoveeHFL(power);
    }

    void Awake(){power = true;}

}
