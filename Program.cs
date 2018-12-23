using System;
using System.IO;
using System.Linq;
using GeometryParser.Model;
using GeometryParser.Serialization;
using GeometryParser.Web;
using HtmlAgilityPack;

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
            var webClient = new WebClient();

            if (File.Exists(xmlPath))
            {
                File.Copy(xmlPath, xmlPath + ".back");
                bikesList = Serializers.Deserialize<BikesList>(xmlPath);
            }

            if (bikesList.Bikes.Count == 0)
            {
                var content = webClient.GetContent("https://geometrygeeks.bike/all-bikes");

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var bikes = doc.DocumentNode.Descendants("tr").Skip(2).Select(t =>
                {
                    var columns = t.SelectNodes("td").ToList();
                    return new Bike
                    {
                        Brand = columns[0].InnerText.Trim(),
                        Model = columns[1].InnerText.Trim(),
                        Year = columns[2].InnerText.Trim(),
                        Url = columns[1].SelectSingleNode("a").Attributes["href"].Value.Trim()
                    };
                });

                bikesList.Bikes.AddRange(bikes);
            }
        }

        #endregion
    }
}