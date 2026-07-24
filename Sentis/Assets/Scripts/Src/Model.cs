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

public class Model : MonoBehaviour
{
    
    public string[] labels;
    public TextAsset labelsAsset;
    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;
    public int maxOutputBoxes = 64;
    public Tensor<float> centersToCorners;
    public ModelAsset asset;
    public Worker engine;
    const BackendType backend = BackendType.GPUCompute;
    public List<string> desiredClasses = new List<string>();

    public void Load()
    {
        labels = labelsAsset.text.Split('\n');
        var model1 = ModelLoader.Load(asset);
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
        
        //Create engine to run model
        engine = new Worker(model1,backend);

    }


}