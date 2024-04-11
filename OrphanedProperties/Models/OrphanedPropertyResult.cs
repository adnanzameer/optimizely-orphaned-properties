namespace OrphanedProperties.Models
{
    public class OrphanedPropertyResult
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string PropertyName { get; set; }
        public bool IsBlockType { get; set; }
        public int PropertyId { get; set; }

        public string Summary { get; set; }
    }
}
