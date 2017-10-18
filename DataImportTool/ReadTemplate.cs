using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ReadDataImportTemplate
{
    public class ReadTemplate
    {
        private readonly XDocument _Xml;

        public ReadTemplate( string path )
        {
            _Xml = XDocument.Load( path );
        }

        public Template Initialize( )
        {
            var template = new Template( );
            var entityCollection = new List < EntityTemplate >( );
            var entity = _Xml.Descendants( "entity" ).Select( ent => new
                                                                     {
                                                                         name = ent.Attribute( "name" )?.Value,
                                                                         count = ent.Attribute( "count" )?.Value,
                                                                         Attributes = ent.Descendants( "attribute" ),
                                                                         ChildEntities = ent.Descendants( "childEntity" )
                                                                     } );

            foreach ( var ent in entity )
            {
                var entityTemp = new EntityTemplate
                                 {
                                     Name = ent.name,
                                     Count = ent.count
                                 };

                //Iterate Attributes
                //TODO fix bug with pulling child attributes
                var attCollection = new List < AttributeTemplate >( );
                foreach ( var attribute in ent.Attributes )
                {
                    if ( attribute.Parent.Attribute( "name" ).Value == entityTemp.Name )
                    {
                        var attTemp = new AttributeTemplate( );
                        attTemp.Name = attribute.Attribute( "name" ) != null ? attribute.Attribute( "name" )?.Value : null;
                        attTemp.Type = attribute.Attribute( "type" ) != null ? attribute.Attribute( "type" )?.Value : null;
                        attTemp.MockarooType = attribute.Attribute( "mockarooType" ) != null ? attribute.Attribute( "mockarooType" )?.Value : null;
                        attTemp.Min = attribute.Attribute( "min" ) != null ? attribute.Attribute( "min" )?.Value : null;
                        attTemp.Max = attribute.Attribute( "max" ) != null ? attribute.Attribute( "max" )?.Value : null;
                        attTemp.Format = attribute.Attribute( "format" ) != null ? attribute.Attribute( "format" )?.Value : null;
                        attTemp.Decimals = attribute.Attribute( "decimals" ) != null ? attribute.Attribute( "decimals" )?.Value : null;
                        attCollection.Add( attTemp );
                    }
                }

                // Iterate Children
                var childEntityCollection = new List < EntityTemplate >( );
                foreach ( var childEntity in ent.ChildEntities )
                {
                    var childXdoc = new XDocument( childEntity );
                    var childTemp = RetrieveChildEntity( childXdoc );
                    childEntityCollection.Add( childTemp );
                }
                entityTemp.ChildrenEntities = childEntityCollection;
                entityTemp.Attributes = attCollection;
                entityCollection.Add( entityTemp );
            }
            template.Entities = entityCollection;
            return template;
        }


        private EntityTemplate RetrieveChildEntity( XDocument childXml )
        {
            var entity = childXml.Descendants( "childEntity" ).Select( ent => new
                                                                              {
                                                                                  name = ent.Attribute( "name" )?.Value,
                                                                                  max = ent.Attribute( "max" )?.Value,
                                                                                  min = ent.Attribute( "min" )?.Value,
                                                                                  parentLookUpName = ent.Attribute( "parentLookUpName" )?.Value,
                                                                                  Attribute = ent.Descendants( "attribute" ),
                                                                                  ChildEntities = ent.Descendants( "childEntity" )
                                                                              } );
            EntityTemplate entityTemp = null;

            foreach ( var ent in entity )
            {
                entityTemp = new EntityTemplate
                             {
                                 Name = ent.name,
                                 Max = ent.max,
                                 Min = ent.min,
                                 ParentLookUpName = ent.parentLookUpName
                             };

                //Iterate Attributes
                var attCollection = new List < AttributeTemplate >( );
                foreach ( var attribute in ent.Attribute )
                {
                    if ( attribute.Parent.Attribute( "name" ).Value == entityTemp.Name )
                    {
                        var attTemp = new AttributeTemplate( );
                        attTemp.Name = attribute.Attribute( "name" ) != null ? attribute.Attribute( "name" )?.Value : null;
                        attTemp.Type = attribute.Attribute( "type" ) != null ? attribute.Attribute( "type" )?.Value : null;
                        attTemp.MockarooType = attribute.Attribute( "mockarooType" ) != null ? attribute.Attribute( "mockarooType" )?.Value : null;
                        attTemp.Min = attribute.Attribute( "min" ) != null ? attribute.Attribute( "min" )?.Value : null;
                        attTemp.Max = attribute.Attribute( "max" ) != null ? attribute.Attribute( "max" )?.Value : null;
                        attTemp.Format = attribute.Attribute( "format" ) != null ? attribute.Attribute( "format" )?.Value : null;
                        attTemp.Decimals = attribute.Attribute( "decimals" ) != null ? attribute.Attribute( "decimals" )?.Value : null;
                        attCollection.Add( attTemp );
                    }
                }

                // Iterate Children
                var childEntityCollection = new List < EntityTemplate >( );
                foreach ( var childEntity in ent.ChildEntities )
                {
                    var childXdoc = new XDocument( childEntity );
                    var childTemp = RetrieveChildEntity( childXdoc );
                    childEntityCollection.Add( childTemp );
                }
                entityTemp.ChildrenEntities = childEntityCollection;
                entityTemp.Attributes = attCollection;
            }
            return entityTemp;
        }
    }
}