using System;
using System.IO;
using System.Xml.Serialization;
using GeometryParser.Model;
using GeometryParser.Serialization;

namespace GeometryParser
{
    class Program
    {
        #region Static members

        static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException();
            var xmlPath = args[0];

            var bikesList = new BikesList();

            if (File.Exists(xmlPath))
            {
                File.Copy(xmlPath, xmlPath + ".back");
                bikesList = Serializers.Deserialize<BikesList>(xmlPath);
            }





        }

        #endregion
    }
}