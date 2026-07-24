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

public class BoundingBox{

    public float centerX;
    public float centerY;
    public float width;
    public float height;
    public string label;
    public float lastUpdatedTime; 

    public BoundingBox(float centerX, float centerY, float width, float height, string label)
    {
        this.centerX = centerX;
        this.centerY = centerY;
        this.width   = width;
        this.height  = height;
        this.label   = label;
        this.lastUpdatedTime = 0;

    }
}
