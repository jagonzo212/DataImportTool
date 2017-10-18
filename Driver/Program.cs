using LoadDataToCrm;
using ReadDataImportTemplate;

namespace Driver
{
    internal class Program
    {
        private static void Main( string [] args )
        {
            var rt = new ReadTemplate( @"C:\Users\Administrator\Desktop\DataImportTool\DataImportTool\Import.xml" );
            var template = rt.Initialize( );

            var cdl = new CrmDataLoad( "AuthType=AD;Url=http://crm:5555/dataimportorg", template );
            cdl.Initialize( );
        }
    }
}