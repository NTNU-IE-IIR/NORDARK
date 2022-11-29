using System.Collections.Generic;

namespace Sampa {
    [System.Serializable]
    public class Timescale
    {
        private string _date;
        private float _delta_t;
        private float _delta_ut1;
        public System.DateTime DateTime
        {
            get => System.DateTime.ParseExact(_date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }
        public float Delta_t
        {
            get => _delta_t;
        }
        public float Delta_ut1
        {
            get => _delta_ut1;
        }

        public Timescale(string date, float delta_t, float delta_ut1)
        {
            _date = date;
            _delta_t = delta_t;
            _delta_ut1 = delta_ut1;
        }
    }

    [System.Serializable]
    public static class Timescales
    {
        public static List<Timescale> timescales = new List<Timescale>{
            new Timescale("1972-01-01", 42.23f, -0.05f),
            new Timescale("1972-07-01", 42.80f, 0.38f),
            new Timescale("1973-01-01", 43.37f, 0.81f),
            new Timescale("1973-07-01", 43.93f, 0.25f),
            new Timescale("1974-01-01", 44.49f, 0.69f),
            new Timescale("1974-07-01", 44.99f, 0.19f),
            new Timescale("1975-01-01", 45.48f, 0.70f),
            new Timescale("1975-07-01", 45.97f, 0.21f),
            new Timescale("1976-01-01", 46.46f, 0.72f),
            new Timescale("1976-07-01", 46.99f, 0.19f),
            new Timescale("1977-01-01", 47.52f, 0.66f),
            new Timescale("1977-07-01", 48.03f, 0.15f),
            new Timescale("1978-01-01", 48.53f, 0.65f),
            new Timescale("1978-07-01", 49.06f, 0.12f),
            new Timescale("1979-01-01", 49.59f, 0.59f),
            new Timescale("1979-07-01", 50.07f, 0.11f),
            new Timescale("1980-01-01", 50.54f, 0.64f),
            new Timescale("1980-07-01", 50.96f, 0.22f),
            new Timescale("1981-01-01", 51.38f, -0.20f),
            new Timescale("1981-07-01", 51.78f, 0.40f),
            new Timescale("1982-01-01", 52.17f, 0.01f),
            new Timescale("1982-07-01", 52.57f, 0.61f),
            new Timescale("1983-01-01", 52.96f, 0.22f),
            new Timescale("1983-07-01", 53.38f, 0.80f),
            new Timescale("1984-01-01", 53.79f, 0.39f),
            new Timescale("1984-07-01", 54.07f, 0.11f),
            new Timescale("1985-01-01", 54.34f, -0.16f),
            new Timescale("1985-07-01", 54.61f, 0.57f),
            new Timescale("1986-01-01", 54.87f, 0.31f),
            new Timescale("1986-07-01", 55.10f, 0.08f),
            new Timescale("1987-01-01", 55.32f, -0.14f),
            new Timescale("1987-07-01", 55.57f, -0.39f),
            new Timescale("1988-01-01", 55.82f, 0.36f),
            new Timescale("1988-07-01", 56.06f, 0.12f),
            new Timescale("1989-01-01", 56.30f, -0.12f),
            new Timescale("1989-07-01", 56.58f, -0.40f),
            new Timescale("1990-01-01", 56.86f, 0.32f),
            new Timescale("1990-07-01", 57.22f, -0.04f),
            new Timescale("1991-01-01", 57.57f, 0.61f),
            new Timescale("1991-07-01", 57.94f, 0.24f),
            new Timescale("1992-01-01", 58.31f, -0.13f),
            new Timescale("1992-07-01", 58.72f, 0.46f),
            new Timescale("1993-01-01", 59.12f, 0.06f),
            new Timescale("1993-07-01", 59.55f, 0.63f),
            new Timescale("1994-01-01", 59.98f, 0.20f),
            new Timescale("1994-07-01", 60.38f, 0.80f),
            new Timescale("1995-01-01", 60.78f, 0.40f),
            new Timescale("1995-07-01", 61.20f, -0.02f),
            new Timescale("1996-01-01", 61.63f, 0.55f),
            new Timescale("1996-07-01", 61.96f, 0.22f),
            new Timescale("1997-01-01", 62.29f, -0.11f),
            new Timescale("1997-07-01", 62.63f, 0.55f),
            new Timescale("1998-01-01", 62.97f, 0.21f),
            new Timescale("1998-07-01", 63.22f, -0.04f),
            new Timescale("1999-01-01", 63.47f, 0.71f),
            new Timescale("1999-07-01", 63.66f, 0.52f),
            new Timescale("2000-01-01", 63.82f, 0.36f),
            new Timescale("2000-07-01", 63.98f, 0.20f),
            new Timescale("2001-01-01", 64.09f, 0.09f),
            new Timescale("2001-07-01", 64.20f, -0.02f),
            new Timescale("2002-01-01", 64.30f, -0.12f),
            new Timescale("2002-07-01", 64.41f, -0.23f),
            new Timescale("2003-01-01", 64.47f, -0.29f),
            new Timescale("2003-07-01", 64.55f, -0.37f),
            new Timescale("2004-01-01", 64.57f, -0.39f),
            new Timescale("2004-07-01", 64.65f, -0.47f),
            new Timescale("2005-01-01", 64.68f, -0.50f),
            new Timescale("2005-07-01", 64.80f, -0.62f),
            new Timescale("2006-01-01", 64.85f, 0.33f),
            new Timescale("2006-07-01", 64.99f, 0.19f),
            new Timescale("2007-01-01", 65.15f, 0.03f),
            new Timescale("2007-07-01", 65.34f, -0.16f),
            new Timescale("2008-01-01", 65.45f, -0.27f),
            new Timescale("2008-07-01", 65.63f, -0.45f),
            new Timescale("2009-01-01", 65.78f, 0.40f),
            new Timescale("2009-07-01", 65.95f, 0.23f),
            new Timescale("2010-01-01", 66.07f, 0.11f),
            new Timescale("2010-07-01", 66.24f, -0.06f),
            new Timescale("2011-01-01", 66.32f, -0.14f),
            new Timescale("2011-07-01", 66.47f, -0.29f),
            new Timescale("2012-01-01", 66.60f, -0.42f),
            new Timescale("2012-07-01", 66.77f, 0.41f),
            new Timescale("2013-01-01", 66.91f, 0.27f),
            new Timescale("2013-07-01", 67.13f, 0.05f),
            new Timescale("2014-01-01", 67.28f, -0.10f),
            new Timescale("2014-07-01", 67.49f, -0.31f),
            new Timescale("2015-01-01", 67.64f, -0.46f),
            new Timescale("2015-07-01", 67.86f, 0.32f),
            new Timescale("2016-01-01", 68.10f, 0.08f),
            new Timescale("2016-07-01", 68.40f, -0.22f),
            new Timescale("2017-01-01", 68.59f, 0.59f),
            new Timescale("2017-07-01", 68.82f, 0.36f),
            new Timescale("2018-01-01", 68.96f, 0.22f),
            new Timescale("2018-07-01", 69.11f, 0.07f),
            new Timescale("2019-01-01", 69.22f, -0.04f),
            new Timescale("2019-07-01", 69.35f, -0.17f),
            new Timescale("2020-01-01", 69.36f, -0.18f),
            new Timescale("2020-07-01", 69.42f, -0.24f),
            new Timescale("2021-01-01", 69.36f, -0.18f),
            new Timescale("2021-07-01", 69.35f, -0.17f),
            new Timescale("2022-01-01", 69.29f, -0.11f),
            new Timescale("2022-07-01", 69.25f, -0.07f)
        };
    }
}