using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ARCustomController : MonoBehaviour
{


    public GameObject objProxyPrefab;

    Vector2 startTouchPosition;
    Vector2 endTouchPosition;

    float rectAreaThresh = 5000f;

    bool drawingRect;

    


    bool IsPointerOverUIObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
          return true;

        for (int touchIndex = 0; touchIndex < Input.touchCount; touchIndex++)
        {
          Touch touch = Input.GetTouch(touchIndex);
          if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return true;
        }

        return false;
    } 

    Texture2D CropToRect(Texture2D fulltexture, Vector2 startPoint, Vector2 endPoint){

        float down = Mathf.Min(UnityEngine.Screen.height - startPoint.y ,UnityEngine.Screen.height - endPoint.y);
        float left = Mathf.Min(startPoint.x,endPoint.x);

        float height = Mathf.Abs(startPoint.y-endPoint.y);
        float width = Mathf.Abs(startPoint.x-endPoint.x);

        int l = (int)left;
        int d = (int)down;
        int w = (int)width;
        int h = (int)height;

        Texture2D res = new Texture2D(w,h);

        res.SetPixels(fulltexture.GetPixels(l,d,w,h));

        res.Apply();

        return res;

    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if(drawingRect &&
           Input.touchCount > 0 &&
           Input.GetTouch(0).phase == TouchPhase.Ended){

            Debug.Log("touch ended: " + GetRectArea(startTouchPosition,endTouchPosition));

            drawingRect = false;

            GameObject newTentative = GameObject.Instantiate(objProxyPrefab, Vector3.zero, Quaternion.identity);

            if(GetRectArea(startTouchPosition,endTouchPosition) < rectAreaThresh){
                Destroy(newTentative);
            }
            else{

                Debug.Log("ALLFIELDS: " + objProxyPrefab.ToString() + " | " + Vector3.zero + " | " + Quaternion.identity);

                Texture2D screencap = ScreenCapture.CaptureScreenshotAsTexture();

                Texture2D croppedWindow = CropToRect(screencap,startTouchPosition,endTouchPosition);
                if(croppedWindow == null){
                    Debug.Log("ERROR NULL CROPPED WINDOW");
                    Destroy(newTentative);
                }
                else{

                    newTentative.GetComponent<SetupObjectProxy>().StoreBoundingBox(startTouchPosition,endTouchPosition);
                    newTentative.GetComponent<ImageQuery>().Texture2DImageOfObject = croppedWindow;
                    newTentative.GetComponent<ImageQuery>().RunAttemptImageQueryHFL();

                }

            }


            

            


        }


        else if(drawingRect &&
           Input.touchCount > 0){

            endTouchPosition = Input.GetTouch(0).position;
        }

        else if(!drawingRect &&
           Input.touchCount > 0 && 
           Input.GetTouch(0).phase == TouchPhase.Began && 
           !IsPointerOverUIObject()){

            Debug.Log("touch started");

            startTouchPosition = Input.GetTouch(0).position;
            endTouchPosition = startTouchPosition;
            drawingRect = true;

        }


    }

    float GetRectArea(Vector2 v1, Vector2 v2){return Mathf.Abs(v1.x-v2.x) * Mathf.Abs(v1.y-v2.y);}

    float f(float x){
        return -(x - UnityEngine.Screen.height/2f) + UnityEngine.Screen.height/2f;
    }


    void OnGUI(){


        if(!drawingRect){return;}

        float down = Mathf.Min(UnityEngine.Screen.height - startTouchPosition.y ,UnityEngine.Screen.height - endTouchPosition.y);
        float left = Mathf.Min(startTouchPosition.x,endTouchPosition.x);

        float height = Mathf.Abs(startTouchPosition.y-endTouchPosition.y);
        float width = Mathf.Abs(startTouchPosition.x-endTouchPosition.x);

        GUI.Box(new Rect(left,down,width,height), "");



    }

}
