using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ReadDataImportTemplate;

namespace MockarooDataGenerator
{
    public class MockarooData
    {
        private const string MockarooKey = "6cf92640";
        private static string Count;
        public static EntityTemplate _EntityTemplate;

        public MockarooData( EntityTemplate entityTemplate )
        {
            if ( entityTemplate.Count != null )
                Count = entityTemplate.Count;
            else
            {
                var randomNumber = new Random( );
                Count = randomNumber.Next( Convert.ToInt32( entityTemplate.Min ), Convert.ToInt32( entityTemplate.Max ) ).ToString( );
            }
            _EntityTemplate = entityTemplate;
        }

        public dynamic Initialize( )
        {
            return GetData( );
        }

        private static dynamic GetData( )
        {
            var request = CreateRequest( );
            dynamic data = null;
            using ( var client = new HttpClient( ) )
            {
                var response = client.SendAsync( request ).Result;
                var json = response.Content.ReadAsStringAsync( ).Result;

                var token = JToken.Parse( json );

                if ( token is JArray )
                    data = JsonConvert.DeserializeObject < List < ExpandoObject > >( json, new ExpandoObjectConverter( ) );
                else if ( token is JObject )
                {
                    dynamic dataholder = JsonConvert.DeserializeObject < ExpandoObject >( json, new ExpandoObjectConverter( ) );
                    var aa = new List < ExpandoObject >( );
                    aa.Add( dataholder );
                    data = aa;
                }
            }

            return data;
        }

        private static string CreateSchema( )
        {
            var fields = new List < MockarooParameter >( );

            foreach ( var attributeTemplate in _EntityTemplate.Attributes )
            {
                if ( attributeTemplate.MockarooType != null )
                {
                    fields.Add( new MockarooParameter
                                {
                                    name = attributeTemplate.Name,
                                    type = attributeTemplate.MockarooType,
                                    min = attributeTemplate.Min,
                                    max = attributeTemplate.Max,
                                    format = attributeTemplate.Format,
                                    decimals = attributeTemplate.Decimals
                                } );
                }
            }

            var jsonObj = JsonConvert.SerializeObject( fields );
            var jsonString = new StringBuilder( jsonObj );
            return jsonString.ToString( );
        }


        private static HttpRequestMessage CreateRequest( )
        {
            var json = CreateSchema( );
            var url = $@"http://www.mockaroo.com/api/generate.json?count={Count}&key={MockarooKey}&fields={json}";

            var request = new HttpRequestMessage
                          {
                              Method = HttpMethod.Get,
                              RequestUri = new Uri( url )
                          };
            return request;
        }
    }
}