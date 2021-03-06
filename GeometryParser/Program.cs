﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using GeometryParser.DAL;
using GeometryParser.DAL.Serialization;
using HtmlAgilityPack;
using WebClient = GeometryParser.Web.WebClient;

namespace GeometryParser
{
    class Program
    {
        #region Static members

        public static void ParseBikes(WebClient webClient, BikesList bikesList)
        {
            webClient.UpdateAll();

            var bikes = bikesList.Bikes;

            var index = Math.Max(0, bikes.FindIndex(b => b.Geometries.Count == 0));

            while (index < bikes.Count)
            {
                var bike = bikes[index];

                if (bike.Geometries.Any())
                {
                    Console.WriteLine($"Skip bike #{index}. It has already data.");

                    index++;
                    continue;
                }

                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q)
                    break;

                Console.WriteLine($"Parse: {index}...");

                
                var content = string.Empty;

                try
                {
                    content = webClient.GetContent("https://geometrygeeks.bike" + bike.Url, 10000);
                }
                catch (WebException ex)
                {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if(errorResponse != null && errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("404 Not Found. Skip.");

                        index++;
                        continue;
                    }

                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        Console.WriteLine("Timeout. Update client...");

                        webClient.UpdateAll();
                        continue;
                    }

                    throw;
                }

                var document = new HtmlDocument();
                document.LoadHtml(content);

                var documentNode = document.DocumentNode;

                var geometries = documentNode.SelectNodes("//th")
                                             ?.Skip(1)
                                             ?.Select(n => new Geometry(n.InnerText.Trim()))
                                             ?.ToList();

                if(geometries == null || geometries.Count == 0)
                {
                    Console.WriteLine("No data. Skip...");
                    index++;
                    continue;
                }

                if (content.Contains("nonsense"))
                {
                    Console.WriteLine($"Data is wrong ({index}). Update client...");

                    webClient.UpdateAll();
                    continue;
                }

                var rows = documentNode.SelectNodes("//tr").Skip(2);

                foreach (var row in rows)
                {
                    var columns = row.SelectNodes("td").ToList();
                    var valueName = columns[0].InnerText.Trim();

                    columns.RemoveAt(0);

                    for (var i = 0; i < columns.Count; i++)
                    {
                        var value = new GeometryValue
                        {
                            Name = valueName,
                            Value = columns[i].InnerText.Trim()
                        };

                        geometries[i].Values.Add(value);
                    }
                }

                bike.Geometries.Clear();
                bike.Geometries.AddRange(geometries);

                webClient.UpdateCookie();
                webClient.UpdateUserAgent();
                index++;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException();
            var xmlPath = args[0];

            var bikesList = new BikesList();
            var webClient = new WebClient();

            if (File.Exists(xmlPath))
            {
                var backPath = xmlPath + ".back";

                if (File.Exists(backPath))
                    File.Delete(backPath);

                File.Copy(xmlPath, xmlPath + ".back");
                bikesList = Serializers.Deserialize<BikesList>(xmlPath);
            }

            if (bikesList.Bikes.Count == 0)
            {
                var content = webClient.GetContent("https://geometrygeeks.bike/all-bikes");

                var document = new HtmlDocument();
                document.LoadHtml(content);

                var bikes = document.DocumentNode.Descendants("tr").Skip(2).Select(t =>
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

            try
            {
                ParseBikes(webClient, bikesList);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.InnerException);
            }
            finally
            {
                Serializers.Serialize(bikesList, xmlPath);
            }
        }

        #endregion
    }
}