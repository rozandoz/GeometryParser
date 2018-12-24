using ConsoleTables;
using GeometryParser.DAL;
using GeometryParser.DAL.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryParser.Viewer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException();

            var bikesList = File.Exists(args[0]) ? Serializers.Deserialize<BikesList>(args[0]) : null;

            var table = new ConsoleTable("bike", "reach", "stack", "size");
            var count = 0;

            foreach (var bike in bikesList.Bikes)
            {
                if (!bike.Year.Contains("2017") && !bike.Year.Contains("2018") && !bike.Year.Contains("2019"))
                    continue;

                var geometries = bike.Geometries;
                if (geometries.Count == 0) continue;

                foreach (var geometry in geometries)
                {
                    var reach = geometry.Values.FirstOrDefault(v => v.Name == "Reach")?.Value;
                    var stack = geometry.Values.FirstOrDefault(v => v.Name == "Stack")?.Value;

                    decimal r, s;
                    if(decimal.TryParse(reach, out r) && decimal.TryParse(stack, out s))
                    {
                        if(r <= 390 && s >= 630)
                        {
                            count++;
                            table.AddRow(bike, reach, stack, geometry.Name);
                        }
                    }
                }

            }

            table.Write();
            Console.ReadKey();
        }
    }
}
