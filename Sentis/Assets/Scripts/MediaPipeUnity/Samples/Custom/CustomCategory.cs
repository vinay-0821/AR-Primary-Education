namespace Mediapipe.Unity.Sample.Custom
{

    public struct CustomCategory
    {
        public readonly int index;
        public readonly float score;
        public readonly string categoryName;
        public readonly string displayName;

        public CustomCategory(int index, float score, string categoryName, string displayName)
        {
            this.index = index;
            this.score = score;
            this.categoryName = categoryName;
            this.displayName = displayName;
        }
    }
}