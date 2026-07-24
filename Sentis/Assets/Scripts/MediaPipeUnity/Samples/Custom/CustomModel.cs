using System;
using System.IO;
using System.Collections.Generic;
using Lays = Unity.Sentis.Layers;
using FF = Unity.Sentis.Functional;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Rendering;
using mtcc = Mediapipe.Tasks.Components.Containers;


namespace Mediapipe.Unity.Sample.ObjectDetection{
public class CustomModel : MonoBehaviour
{
    private string[] labels;
    public TextAsset labelsAssets;
    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.85f;
    public ModelAsset modelAsset;
    public Worker engine;
    public Tensor<float> centersToCorners;
    const BackendType backend = BackendType.GPUCompute;
    private const int imageWidth = 640;
    private const int imageHeight = 640;

   public void Awake()
   {
       Load();
   }

    public void Load()
    {
        labels = labelsAssets.text.Split('\n');
        var model1 = ModelLoader.Load(modelAsset);
        centersToCorners = new Tensor<float>(new TensorShape(4, 4),
        new float[]
        {
                    1,      0,      1,      0,
                    0,      1,      0,      1,
                    -0.5f,  0,      0.5f,   0,
                    0,      -0.5f,  0,      0.5f
        });


        //Here we transform the output of the model1 by feeding it through a Non-Max-Suppression layer.
        var graph = new FunctionalGraph();
        var input = graph.AddInput(model1,0);
        var modelOutput = Functional.Forward(model1, input)[0];
        var boxCoords = modelOutput[0, 0..4, ..].Transpose(0, 1);        //shape=(8400,4)
        var allScores = modelOutput[0, 4.., ..];                         //shape=(80,8400)
        var scores = FF.ReduceMax(allScores, 0);        //shape=(8400)
        var classIDs = FF.ArgMax(allScores, 0);                          //shape=(8400) 
        var boxCorners = FF.MatMul(boxCoords, Functional.Constant(centersToCorners));
        var indices = FF.NMS(boxCorners, scores, iouThreshold, scoreThreshold);           //shape=(N)
        var indices2 = indices.Unsqueeze(-1).BroadcastTo(new int[] { 4 });//shape=(N,4)
        var coords = FF.Gather(boxCoords, 0, indices2);                  //shape=(N,4)
        var labelIDs = FF.Gather(classIDs, 0, indices);                  //shape=(N)
        model1 = graph.Compile(coords,labelIDs);

        engine = new Worker(model1,backend);
        Debug.Log("Model Loaded SuccessFully");
    }

    public mtcc.DetectionResult Detect(Texture texture)
    {
        using var input = TextureConverter.ToTensor(texture, imageWidth, imageHeight, 3);
        engine.Schedule(input);
        
        var outputGPU   =  engine.PeekOutput("output_0") as Tensor<float>;
        var labelIDsGPU =  engine.PeekOutput("output_1") as Tensor<int>;

        var output = outputGPU.ReadbackAndClone();
        var labelIDs = labelIDsGPU.ReadbackAndClone();

        int boxesFound = output.shape[0];

        var imageSource = ImageSourceProvider.ImageSource;
        int displayWidth = imageSource.textureWidth;
        int displayHeight = imageSource.textureHeight;

        float scaleX = displayWidth/imageWidth;
        float scaleY = displayHeight/imageHeight;

        List<mtcc.Detection> detections = new List<mtcc.Detection>();

        for(int i=0; i<boxesFound; i++)
        {
           List<mtcc.Category> categories = new List<mtcc.Category>();
           mtcc.Category category = new mtcc.Category(labelIDs[i],0.95f,labels[labelIDs[i]],labels[labelIDs[i]]);
           categories.Add(category);
           
           List<mtcc.NormalizedKeypoint> keypoints = new List<mtcc.NormalizedKeypoint>();
          


           float x_center    =  (output[i, 0]); 
           float y_center    =  (imageHeight - output[i, 1]);
           float width       =  (output[i, 2]);
           float height      =  (output[i, 3]);

           float offset = (displayHeight - imageHeight * scaleX) / 2;
           offset = 20;


           int left = (int) ((x_center - width/2)*scaleX);
           int right = (int) (left + width*scaleX);
           int top = (int) ((y_center - height/2)*scaleY + offset);
           int bottom = (int) (top + height*scaleY + offset);

        //    Debug.Log($"{output[i,0]}, {output[i,1]}, {output[i,2]}, {output[i,3]}");
           // x_center , image-y_center , width , height
        //    Debug.Log($"left :{left},  top :{top}, bottom :{bottom},right :{right}");



        //    mtcc.Rect boundingBox = new mtcc.Rect(x_center,y_center,x_center+width,y_center+height);
           mtcc.Rect boundingBox = new mtcc.Rect(left,top,right,bottom);
           mtcc.Detection detection = new mtcc.Detection(categories,boundingBox,keypoints);
           detections.Add(detection);
           
        }

        mtcc.DetectionResult detectionResult = new mtcc.DetectionResult(detections);
        return detectionResult;
    }



    

     private void OnDestroy()
    {
        centersToCorners?.Dispose();
        engine?.Dispose();
    }


}

}