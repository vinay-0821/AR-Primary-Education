using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity.Sample.Custom{
    
    public struct CustomDetection{
        private const int _DefaultCategoryIndex = -1;
        public readonly List<CustomCategory> categories;
        public readonly CustomRect boundingBox;
        public readonly List<NormalizedKeypoint> keypoints;

        public  CustomDetection(List<CustomCategory> categories, CustomRect boundingBox, List<NormalizedKeypoint> keypoints)
        {
            this.categories = categories;
            this.boundingBox = boundingBox;
            this.keypoints = keypoints;
        }
        
    }

    public struct CustomDetectionResult{

        public readonly List<CustomDetection> detections;

        public CustomDetectionResult(List<CustomDetection> detections)
        {
             this.detections = detections;
        }

    }
}