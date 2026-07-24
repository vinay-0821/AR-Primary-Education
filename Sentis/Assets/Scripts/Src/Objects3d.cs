using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Lays = Unity.Sentis.Layers;
using System.IO;
using FF = Unity.Sentis.Functional;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using UnityEngine.Android;
using System;
using UnityEngine.Timeline;


public class Objects3d : MonoBehaviour
{
    public float fixedDepth = -500.0f;
    public List<string> ObjectsLabels;
    public List<GameObject> ObjectsPrefs;  //make it list of lists
    public Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();
    List<GameObject> boxPool = new();
    List<GameObject> ObjectPool = new();

    private void Awake()
    {
         for (int i = 0; i < ObjectsLabels.Count; i++)            
        {
            map.Add(ObjectsLabels[i], ObjectsPrefs[i]);
        }
    } 

    public void DrawBox(BoundingBox box,int id, float fontSize, Transform displayLocation, Sprite borderSprite)
    {
        GameObject panel;
        GameObject object3d;

        if(id < boxPool.Count)
        {
            panel = boxPool[id];
            panel.SetActive(true);
            object3d = ObjectPool[id];
            object3d.SetActive(true);
        }
        else
        {
            panel = CreateNewBox(Color.yellow,displayLocation, borderSprite);
            object3d = CreateNewObject3d(box,displayLocation);
        }

        object3d.SetActive(true);

        if(map.ContainsKey(box.label))
        {
            GameObject correctPrefab = map[box.label];
            if(object3d.name != correctPrefab.name + "(Clone)")
            {
                Destroy(object3d);
                object3d = Instantiate(correctPrefab, displayLocation);
                ObjectPool[id] = object3d;
            }

        }
        else{

            Debug.LogError("Prefab not found for label: " + box.label);
            return;  // Skip spawning if prefab is missing

        }

        float verticalOffset = 0;
        Vector3 scaleAndDepth = CalculateScaleAndDepth(box.width, box.height);

        panel.transform.localPosition = new Vector3(box.centerX, -box.centerY);
        object3d.transform.localPosition = new Vector3(box.centerX, -box.centerY + verticalOffset, 0);
        object3d.transform.localScale = new Vector3(scaleAndDepth.x, scaleAndDepth.x, scaleAndDepth.x);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(box.width, box.height);

        //set Label text
        Text label = panel.GetComponentInChildren<Text>();
        label.text = box.label;
        label.fontSize = (int)fontSize;

    }  

     Vector3 CalculateScaleAndDepth(float width, float height)
    {
        // Constants to define how scale and depth relate to the bounding box size
        float baseScale = 1f;  // Base scale factor
        float depthFactor = 0.1f;  // Factor to calculate depth from size

        // Calculate scale as a function of the average of width and height
        float scale = baseScale * (width + height) / 2;

        // Calculate depth inversely proportional to the size of the bounding box
        float depth = fixedDepth - depthFactor * (width + height);

        return new Vector3(scale, scale, depth);
    } 

    public GameObject CreateNewBox(Color color, Transform displayLocation, Sprite borderSprite)
    {
        var panel = new GameObject("ObjectBox");
        panel.AddComponent<CanvasRenderer>();
        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        img.sprite = borderSprite;
        img.type = Image.Type.Sliced;
        panel.transform.SetParent(displayLocation, false);

        var text = new GameObject("ObjectLabel");
        text.AddComponent<CanvasRenderer>();
        text.transform.SetParent(panel.transform, false);
        Text txt = text.AddComponent<Text>();
      //  txt.font = font; 
      
        txt.color = color;
        txt.fontSize = 40;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        RectTransform rt2 = text.GetComponent<RectTransform>();
        rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
        rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
        rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
        rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
        rt2.anchorMin = new Vector2(0, 0);
        rt2.anchorMax = new Vector2(1, 1);

        boxPool.Add(panel);
        return panel;
    }

     public GameObject CreateNewObject3d(BoundingBox box, Transform displayLocation)
    {
        //Create the box and set image

        var object3d = new GameObject();

        Debug.Log(box.label);

        object3d = Instantiate(map[box.label]);

        object3d.transform.SetParent(displayLocation, false);

        ObjectPool.Add(object3d);

        return object3d;
    }

        public void ClearAnnotations()
    {
        foreach (var box in boxPool)
        {
            box.SetActive(false);
        }
        foreach (var obj in ObjectPool)
        {
            obj.SetActive(false);
        }
    }
}