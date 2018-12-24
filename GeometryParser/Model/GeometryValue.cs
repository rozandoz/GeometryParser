using System;

namespace GeometryParser.Model
{
    [Serializable]
    public class GeometryValue
    {
        #region Properties

        public string Name { get; set; }
        public string Value { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }

        #endregion
    }
}