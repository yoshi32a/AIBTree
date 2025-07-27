using System.Collections.Generic;

namespace ArcBT.TagSystem
{
    [System.Serializable]
    public class TagCategory
    {
        public string categoryName;
        public string description;
        public List<TagDefinition> tags = new List<TagDefinition>();
    }
}