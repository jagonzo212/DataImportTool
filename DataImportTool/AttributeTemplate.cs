using Microsoft.Xrm.Sdk.Metadata;

namespace ReadDataImportTemplate
{
    public class AttributeTemplate
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string MockarooType { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Format { get; set; }
        public string Decimals { get; set; }
        public AttributeMetadata AttMetadata { get; set; }
    }
}