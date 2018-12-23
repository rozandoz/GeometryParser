using System.IO;
using System.Xml.Serialization;

namespace GeometryParser.Serialization
{
    public static class Serializers
    {
        #region Static members

        public static T Deserialize<T>(string filePath) where T : class
        {
            using (var file = File.OpenRead(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(file) as T;
            }
        }

        public static void Serialize(object obj, string filePath)
        {
            using (var file = File.OpenWrite(filePath))
            {
                var serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(file, obj);
            }
        }

        #endregion
    }
}