using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.Connector;
using MockarooDataGenerator;
using ReadDataImportTemplate;

namespace LoadDataToCrm
{
    public class CrmDataLoad
    {
        private readonly Template Template;
        private CrmServiceClient CrmService;

        public CrmDataLoad( string connectionString,
                            Template template )
        {
            CreateCrmService( connectionString );
            Template = template;
        }

        public void Initialize( )
        {
            foreach ( var entityTemplate in Template.Entities )
                IterateEntities( entityTemplate );
        }

        private void IterateEntities( EntityTemplate entityTemplate )
        {
            // retrieve metadata for the template
            entityTemplate.Attributes = RetrieveMetadata( entityTemplate );
            var mockarooData = new MockarooData( entityTemplate );
            var mockarooDataSet = mockarooData.Initialize( );
            var parentIds = CreateRecords( entityTemplate, entityTemplate.Attributes, mockarooDataSet );

            //Iterate children
            foreach ( var childEntity in entityTemplate.ChildrenEntities )
                IterateEntities( childEntity, parentIds, entityTemplate.Name );
        }

        // Iterates children
        private void IterateEntities( EntityTemplate entityTemplate,
                                      List < Guid > parentIds,
                                      string parentSchemaName )
        {
            entityTemplate.Attributes = RetrieveMetadata( entityTemplate );

            foreach ( var parentId in parentIds )
            {
                var mockarooData = new MockarooData( entityTemplate );
                var mockarooDataSet = mockarooData.Initialize( );
                var newParentId = CreateRecords( entityTemplate, entityTemplate.Attributes, mockarooDataSet, parentId, parentSchemaName );

                foreach ( var childEntity in entityTemplate.ChildrenEntities )
                    IterateEntities( childEntity, newParentId, entityTemplate.Name );
            }
        }

        private void CreateCrmService( string connectionString )
        {
            CrmService = new CrmServiceClient( connectionString );
        }

        private List < Guid > CreateRecords( EntityTemplate entityTemplate,
                                             List < AttributeTemplate > attributes,
                                             dynamic mockarooDataSet )
        {
            var entity = new Entity( entityTemplate.Name );
            var ids = new List < Guid >( );
            foreach ( IDictionary < string, object > mockaroodata in mockarooDataSet )
            {
                var createRecord = false;
                for ( var i = 0; i < attributes.Count; i++ )
                {
                    if ( mockaroodata.ContainsKey( attributes[ i ].Name ) )
                    {
                        createRecord = true;
                        entity[ attributes[ i ].Name ] = CastStringToObject( attributes[ i ], mockaroodata[ attributes[ i ].Name ].ToString( ) );
                    }
                    else if ( attributes[ i ].AttMetadata.AttributeType.ToString( ) == "Picklist" )
                    {
                        createRecord = true;
                        entity[ attributes[ i ].Name ] = CastStringToObject( attributes[ i ], string.Empty );
                    }
                }
                if ( createRecord )
                    ids.Add( CrmService.Create( entity ) );
            }

            return ids;
        }

        private List < Guid > CreateRecords( EntityTemplate entityTemplate,
                                             List < AttributeTemplate > attributes,
                                             dynamic mockarooDataSet,
                                             Guid parentId,
                                             string parentEntityName )
        {
            var entity = new Entity( entityTemplate.Name );
            var ids = new List < Guid >( );
            foreach ( IDictionary < string, object > mockaroodata in mockarooDataSet )
            {
                var createRecord = false;
                for ( var i = 0; i < attributes.Count; i++ )
                {
                    if ( mockaroodata.ContainsKey( attributes[ i ].Name ) )
                    {
                        createRecord = true;
                        entity[ attributes[ i ].Name ] = CastStringToObject( attributes[ i ], mockaroodata[ attributes[ i ].Name ].ToString( ) );
                    }
                    else if ( attributes[ i ].AttMetadata.AttributeType.ToString( ) == "Picklist" )
                    {
                        createRecord = true;
                        entity[ attributes[ i ].Name ] = CastStringToObject( attributes[ i ], string.Empty );
                    }
                }
                if ( createRecord )
                {
                    entity[ entityTemplate.ParentLookUpName ] = new EntityReference( parentEntityName, parentId );
                    ids.Add( CrmService.Create( entity ) );
                }
            }


            return ids;
        }

        private List < AttributeTemplate > RetrieveMetadata( EntityTemplate entityTemplate )
        {
            // TODO Set MockarooType here
            var entityMetadataRequest = new RetrieveEntityRequest
                                        {
                                            LogicalName = entityTemplate.Name,
                                            EntityFilters = EntityFilters.Attributes
                                        };
            var metadata = ( RetrieveEntityResponse ) CrmService.Execute( entityMetadataRequest );

            foreach ( var attributeTemplate in entityTemplate.Attributes )
            {
                foreach ( var attributeMetadata in metadata.EntityMetadata.Attributes )
                {
                    if ( attributeTemplate.Name == attributeMetadata.LogicalName )
                    {
                        attributeTemplate.AttMetadata = attributeMetadata;

                        // Set Mockaroo default details if it's not already set
                        if ( attributeTemplate.MockarooType == null )
                        {
                            switch ( attributeTemplate.AttMetadata.AttributeType.ToString( ) )
                            {
                                case "BigInt":
                                    attributeTemplate.MockarooType = "Number";
                                    attributeTemplate.Format = "0";
                                    break;
                                case "Boolean":
                                    attributeTemplate.MockarooType = "Boolean";
                                    break;
                                case "DateTime":
                                    attributeTemplate.MockarooType = "Date";
                                    attributeTemplate.Min = "1/1/1950";
                                    attributeTemplate.Max = DateTime.Now.ToString( "MM/dd/yyyy" );
                                    attributeTemplate.Format = "%m/%d/%Y";
                                    break;
                                case "Integer":
                                    attributeTemplate.MockarooType = "Number";
                                    attributeTemplate.Decimals = "0";
                                    attributeTemplate.Min = "0";
                                    attributeTemplate.Max = "10000";
                                    break;
                                case "Memo":
                                    attributeTemplate.MockarooType = "Words";
                                    attributeTemplate.Min = "50";
                                    attributeTemplate.Max = "150";
                                    break;
                                case "String":
                                    attributeTemplate.MockarooType = "Words";
                                    attributeTemplate.Min = "5";
                                    attributeTemplate.Max = "12";
                                    break;
                                case "Money":
                                    attributeTemplate.MockarooType = "Number";
                                    attributeTemplate.Decimals = "2";
                                    attributeTemplate.Min = "0";
                                    attributeTemplate.Max = "1000000";
                                    break;
                                case "Decimal":
                                    attributeTemplate.MockarooType = "Number";
                                    attributeTemplate.Decimals = "2";
                                    attributeTemplate.Min = "0";
                                    attributeTemplate.Max = "10000";
                                    break;
                                case "Double":
                                    attributeTemplate.MockarooType = "Number";
                                    attributeTemplate.Decimals = "2";
                                    attributeTemplate.Min = "0";
                                    attributeTemplate.Max = "10000";
                                    break;
                            }
                        }
                    }
                }
            }
            return entityTemplate.Attributes;
        }

        private object CastStringToObject( AttributeTemplate attributeTemplate,
                                           string value )
        {
            switch ( attributeTemplate.AttMetadata.AttributeType.ToString( ) )
            {
                // TODO add decimal
                case "BigInt":
                    return Convert.ToInt64( value );
                case "Boolean":
                    return Convert.ToBoolean( value );
                case "DateTime":
                    return Convert.ToDateTime( value );
                case "Integer":
                    return Convert.ToInt32( value );
                case "Memo":
                    return value;
                case "Picklist":
                    return new OptionSetValue( SelectOptionFromOptionSet( attributeTemplate ) );
                case "String":
                    return value;
                case "Money":
                    return new Money( Convert.ToDecimal( value ) );
                case "Decimal":
                    return Convert.ToDecimal( value );
                case "Double":
                    return Convert.ToDouble( value );
                case "Customer":
                case "Lookup":
                case "State":
                case "Status":
                    break;
            }
            throw new Exception( "Object type unusable " + attributeTemplate.Name + " " + attributeTemplate.AttMetadata.AttributeType );
        }

        private int SelectOptionFromOptionSet( AttributeTemplate attributeTemplate )
        {
            var optionSetMetadata = ( EnumAttributeMetadata ) attributeTemplate.AttMetadata;

            var optionList = ( from o in optionSetMetadata.OptionSet.Options
                               select new {o.Value, Text = o.Label.UserLocalizedLabel.Label} ).ToList( );

            var randomNumber = new Random( );
            return Convert.ToInt32( optionList[ randomNumber.Next( 0, optionList.Count - 1 ) ].Value );
        }
    }
}