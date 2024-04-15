namespace AJKIOT.Shared.Models
{
    public enum FeatureType
    {
        Switch = 1,
        OpenTimer,
    }

    public class FeatureTypeSelect
    {
        public int Id { get; set; }
        public FeatureType Type { get; set; }
    }
}
