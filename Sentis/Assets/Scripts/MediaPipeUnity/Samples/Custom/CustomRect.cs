using System;


namespace Mediapipe.Unity.Sample.Custom{

    public struct CustomRect{
       
        public readonly int left;
        public readonly int top;
        public readonly int right;
        public readonly int bottom;

       public CustomRect(int left, int top, int right, int bottom)
       {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
       }
    }
}