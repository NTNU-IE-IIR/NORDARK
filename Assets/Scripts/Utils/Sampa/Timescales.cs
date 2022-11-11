using System.Collections.Generic;

namespace Sampa {
    [System.Serializable]
    public class Timescales
    {
        public List<Timescale> timescales;
    }

    [System.Serializable]
    public class Timescale
    {
        public string date;
        public float delta_t;
        public float delta_ut1;
        public System.DateTime dateTime
        {
            get
            {
                return System.DateTime.ParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}