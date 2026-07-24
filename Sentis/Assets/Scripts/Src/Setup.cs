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


public class Setup : MonoBehaviour{

    public RawImage rawImage;
    public Sprite borderSprite;
    public Texture2D borderTexture;
    public Font font;
    public WebCamTexture webcamTexture;
    public Material blendMaterial = null;
    private const int imageWidth = 640;
    private const int imageHeight = 640;
    private VideoPlayer video;
    private RenderTexture targetRT;
    public RenderTexture someRenderTexture;  
    private Transform displayLocation;
    public Model model;
    public Objects3d objects3d;


   private void Awake()
   {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        model.Load();
        targetRT = new RenderTexture(imageWidth, imageHeight, 0);

        displayLocation = rawImage.transform;
        SetupWebcam();
        SetupInput();
        
        if (borderSprite == null)
        {
            borderSprite = Sprite.Create(borderTexture, new Rect(0, 0, borderTexture.width, borderTexture.height), new Vector2(borderTexture.width / 2, borderTexture.height / 2));
        }
        
   }

    void SetupWebcam()
    {
        webcamTexture = new WebCamTexture();
        rawImage.texture = webcamTexture;
        webcamTexture.Play();
   }

    void SetupInput() 
    {
        video = gameObject.AddComponent<VideoPlayer>();
        video.renderMode = VideoRenderMode.APIOnly;
        video.source = VideoSource.Url;
        video.isLooping = true;
        video.Play();
    }


    private void Start()
    {
        // Request camera permission for Android
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
        }

    }

    private void Update()
    {
        if(webcamTexture.didUpdateThisFrame)
        {
            /*TODO*/
            ExecuteML();
        }

        blendMaterial.SetTexture("_MainTex", webcamTexture);
        blendMaterial.SetTexture("_OverlayTex", someRenderTexture);

    }

    public void ExecuteML()
    {
        objects3d.ClearAnnotations();

        if(webcamTexture && webcamTexture.width > 100)
        {
            float aspect = webcamTexture.width * 1f / webcamTexture.height;
            Graphics.Blit(webcamTexture, targetRT, new Vector2(1f / aspect, 1), new Vector2(0, 0));
            rawImage.texture = targetRT;
        }
        else return;

        using var input = TextureConverter.ToTensor(targetRT, imageWidth, imageHeight, 3); 
        model.engine.Schedule(input);

        var outputGPU = model.engine.PeekOutput("output_0") as Tensor<float>;
        var labelIDsGPU = model.engine.PeekOutput("output_1") as Tensor<int>;

        var output = outputGPU.ReadbackAndClone();
        var labelIDs = labelIDsGPU.ReadbackAndClone();

        float displayWidth = rawImage.rectTransform.rect.width;
        float displayHeight = rawImage.rectTransform.rect.height;

        float scaleX = displayWidth / imageWidth;
        float scaleY = displayHeight / imageHeight;

        int boxesFound = output.shape[0];

        for (int n = 0; n < Mathf.Min(boxesFound, model.maxOutputBoxes); n++)
        {
            string detectedLabel = model.labels[labelIDs[n]];
            if (model.desiredClasses.Contains(detectedLabel))
            {
                Debug.Log(detectedLabel);
                var box = new BoundingBox
                (
                   output[n, 0] * scaleX - displayWidth / 2,
                   output[n, 1] * scaleY - displayHeight / 2,
                   output[n, 2] * scaleX,
                   output[n, 3] * scaleY,
                   model.labels[labelIDs[n]]

                );

                Debug.Log(box);
                objects3d.DrawBox(box, n, displayHeight * 0.05f,displayLocation, borderSprite);
            }
        }
    }

     private void OnDestroy()
    {
        // Dispose resources and stop webcam
        webcamTexture.Stop();
        model.centersToCorners?.Dispose();
        model.engine?.Dispose();
    }

    



}