using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using OnTrack.IO.CSV;
using System.Diagnostics;

namespace OnTrack.Testing
{
    [TestClass]
    public class CSVTesting
    {
        [TestMethod]
        public void MainTesting()
        {
            /*
             *  Dim uri As System.Uri = New System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            ''' select the direct path 
            If String.IsNullOrWhiteSpace(searchpath) Then
                searchpath = My.Application.Info.DirectoryPath & "\Resources\" & DefaultFolder
            End If
             */
            System.Uri uri = new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
           String path = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\15_ObjectMessageTypes.csv";
            System.IO.StreamReader aStream = new System.IO.StreamReader (path);
            OnTrack.IO.CSV.Reader aReader = new OnTrack.IO.CSV.Reader(aStream, delimiter:";");
            if (!aReader.Process ()){
                
            }
            else {
                   Console.Write( aReader.ToString());
            }
            Console.Write("done");
        }
    }
}
