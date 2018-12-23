using System;

namespace GeometryParser.Model
{
    [Serializable]
    public class Bike
    {
        #region Properties

        public string Brand { get; set; }
        public string Model { get; set; }
        public string Url { get; set; }
        public string Year { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"{Brand} {Model} ({Year})";
        }

        #endregion
    }
}