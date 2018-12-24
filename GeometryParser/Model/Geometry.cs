using System;
using System.Collections.Generic;

namespace GeometryParser.Model
{
    [Serializable]
    public class Geometry
    {
        #region Constructors

        public Geometry()
        {
            Values = new List<GeometryValue>();
        }

        public Geometry(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; set; }
        public List<GeometryValue> Values { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"{Name}: {Values.Count} geometry values";
        }

        #endregion
    }
}