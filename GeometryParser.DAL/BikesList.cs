using System;
using System.Collections.Generic;

namespace GeometryParser.DAL
{
    [Serializable]
    public class BikesList
    {
        #region Constructors

        public BikesList()
        {
            Bikes = new List<Bike>();
        }

        #endregion

        #region Properties

        public List<Bike> Bikes { get; set; }

        #endregion
    }
}