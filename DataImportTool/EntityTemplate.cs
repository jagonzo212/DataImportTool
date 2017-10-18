using System.Collections.Generic;

namespace ReadDataImportTemplate
{
    public class EntityTemplate
    {
        public string Name { get; set; }
        public string Count { get; set; }
        public List < AttributeTemplate > Attributes { get; set; }
        public List < EntityTemplate > ChildrenEntities { get; set; }
        public string ParentLookUpName { get; set; }
        public string Max { get; set; }
        public string Min { get; set; }
    }
}