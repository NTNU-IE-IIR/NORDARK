/*
See https://midcdmz.nrel.gov/sampa/ for more information
*/

namespace Sampa {
    public class Sampa
    {
        //----------------------INPUT VALUES------------------------
        public int year;                // 4-digit year,      valid range: -2000 to 6000
        public int month;               // 2-digit month,         valid range: 1 to  12
        public int day;                 // 2-digit day,           valid range: 1 to  31
        public int hour;                // Observer local hour,   valid range: 0 to  24
        public int minute;              // Observer local minute, valid range: 0 to  59
        public double second;           // Observer local second, valid range: 0 to <60
        public double delta_ut1;        // Fractional second difference between UTC and UT which is used
                                        // to adjust UTC for earth's irregular rotation rate and is derived
                                        // from observation only and is reported in this bulletin:
                                        // https://www.stjarnhimlen.se/comp/time.html
                                        // valid range: -1 to 1 second
        public double delta_t;          // Difference between earth rotation time and terrestrial time
                                        // It is derived from observation only and is reported in this
                                        // bulletin: https://www.stjarnhimlen.se/comp/time.html,
                                        // valid range: -8000 to 8000 seconds
        public double timezone;         // Observer time zone (negative west of Greenwich)
                                        // valid range: -18   to   18 hours
        public double longitude;        // Observer longitude (negative west of Greenwich)
                                        // valid range: -180  to  180 degrees
        public double latitude;         // Observer latitude (negative south of equator)
                                        // valid range: -90   to   90 degrees
        public double elevation;        // Observer elevation [meters]
                                        // valid range: -6500000 or higher meters
        public double pressure;         // Annual average local pressure [millibars]
                                        // valid range:    0 to 5000 millibars
        public double temperature;      // Annual average local temperature [degrees Celsius]
                                        // valid range: -273 to 6000 degrees Celsius
        public double atmos_refract;    // Atmospheric refraction at sunrise and sunset (0.5667 deg is typical)
                                        // valid range: -5   to   5 degrees
        //---------------------Final OUTPUT VALUES------------------------
        public Mpa mpa;     // Moon
        public Spa spa;     // Sun

        public void Calculate()
        {
            spa = new Spa();
            mpa = new Mpa();

            spa.Calculate(this);
            mpa.Calculate(this, spa);
        }
    }
}