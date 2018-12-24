using System;
using System.Collections.Generic;

namespace GeometryParser.DAL
{
    [Serializable]
    public class Bike
    {
        #region Constructors

        public Bike()
        {
            Geometries = new List<Geometry>();
        }

        #endregion

        #region Properties

        public string Brand { get; set; }

        public List<Geometry> Geometries { get; set; }
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