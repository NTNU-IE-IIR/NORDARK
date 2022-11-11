namespace Sampa {
    public class Mpa
    {
        const int COUNT = 60;
        enum TERM {TERM_D, TERM_M, TERM_MPR, TERM_F, TERM_LB, TERM_R, TERM_COUNT};
        double[][] ML_TERMS = new double[][] {
            new double[] {0,0,1,0,6288774,-20905355},
            new double[] {2,0,-1,0,1274027,-3699111},
            new double[] {2,0,0,0,658314,-2955968},
            new double[] {0,0,2,0,213618,-569925},
            new double[] {0,1,0,0,-185116,48888},
            new double[] {0,0,0,2,-114332,-3149},
            new double[] {2,0,-2,0,58793,246158},
            new double[] {2,-1,-1,0,57066,-152138},
            new double[] {2,0,1,0,53322,-170733},
            new double[] {2,-1,0,0,45758,-204586},
            new double[] {0,1,-1,0,-40923,-129620},
            new double[] {1,0,0,0,-34720,108743},
            new double[] {0,1,1,0,-30383,104755},
            new double[] {2,0,0,-2,15327,10321},
            new double[] {0,0,1,2,-12528,0},
            new double[] {0,0,1,-2,10980,79661},
            new double[] {4,0,-1,0,10675,-34782},
            new double[] {0,0,3,0,10034,-23210},
            new double[] {4,0,-2,0,8548,-21636},
            new double[] {2,1,-1,0,-7888,24208},
            new double[] {2,1,0,0,-6766,30824},
            new double[] {1,0,-1,0,-5163,-8379},
            new double[] {1,1,0,0,4987,-16675},
            new double[] {2,-1,1,0,4036,-12831},
            new double[] {2,0,2,0,3994,-10445},
            new double[] {4,0,0,0,3861,-11650},
            new double[] {2,0,-3,0,3665,14403},
            new double[] {0,1,-2,0,-2689,-7003},
            new double[] {2,0,-1,2,-2602,0},
            new double[] {2,-1,-2,0,2390,10056},
            new double[] {1,0,1,0,-2348,6322},
            new double[] {2,-2,0,0,2236,-9884},
            new double[] {0,1,2,0,-2120,5751},
            new double[] {0,2,0,0,-2069,0},
            new double[] {2,-2,-1,0,2048,-4950},
            new double[] {2,0,1,-2,-1773,4130},
            new double[] {2,0,0,2,-1595,0},
            new double[] {4,-1,-1,0,1215,-3958},
            new double[] {0,0,2,2,-1110,0},
            new double[] {3,0,-1,0,-892,3258},
            new double[] {2,1,1,0,-810,2616},
            new double[] {4,-1,-2,0,759,-1897},
            new double[] {0,2,-1,0,-713,-2117},
            new double[] {2,2,-1,0,-700,2354},
            new double[] {2,1,-2,0,691,0},
            new double[] {2,-1,0,-2,596,0},
            new double[] {4,0,1,0,549,-1423},
            new double[] {0,0,4,0,537,-1117},
            new double[] {4,-1,0,0,520,-1571},
            new double[] {1,0,-2,0,-487,-1739},
            new double[] {2,1,0,-2,-399,0},
            new double[] {0,0,2,-2,-381,-4421},
            new double[] {1,1,1,0,351,0},
            new double[] {3,0,-2,0,-340,0},
            new double[] {4,0,-3,0,330,0},
            new double[] {2,-1,2,0,327,0},
            new double[] {0,2,1,0,-323,1165},
            new double[] {1,1,-1,0,299,0},
            new double[] {2,0,3,0,294,0},
            new double[] {2,0,-1,-2,0,8752}
        };
        double[][] MB_TERMS = new double[][] {
            new double[] {0,0,0,1,5128122,0},
            new double[] {0,0,1,1,280602,0},
            new double[] {0,0,1,-1,277693,0},
            new double[] {2,0,0,-1,173237,0},
            new double[] {2,0,-1,1,55413,0},
            new double[] {2,0,-1,-1,46271,0},
            new double[] {2,0,0,1,32573,0},
            new double[] {0,0,2,1,17198,0},
            new double[] {2,0,1,-1,9266,0},
            new double[] {0,0,2,-1,8822,0},
            new double[] {2,-1,0,-1,8216,0},
            new double[] {2,0,-2,-1,4324,0},
            new double[] {2,0,1,1,4200,0},
            new double[] {2,1,0,-1,-3359,0},
            new double[] {2,-1,-1,1,2463,0},
            new double[] {2,-1,0,1,2211,0},
            new double[] {2,-1,-1,-1,2065,0},
            new double[] {0,1,-1,-1,-1870,0},
            new double[] {4,0,-1,-1,1828,0},
            new double[] {0,1,0,1,-1794,0},
            new double[] {0,0,0,3,-1749,0},
            new double[] {0,1,-1,1,-1565,0},
            new double[] {1,0,0,1,-1491,0},
            new double[] {0,1,1,1,-1475,0},
            new double[] {0,1,1,-1,-1410,0},
            new double[] {0,1,0,-1,-1344,0},
            new double[] {1,0,0,-1,-1335,0},
            new double[] {0,0,3,1,1107,0},
            new double[] {4,0,0,-1,1021,0},
            new double[] {4,0,-1,1,833,0},
            new double[] {0,0,1,-3,777,0},
            new double[] {4,0,-2,1,671,0},
            new double[] {2,0,0,-3,607,0},
            new double[] {2,0,2,-1,596,0},
            new double[] {2,-1,1,-1,491,0},
            new double[] {2,0,-2,1,-451,0},
            new double[] {0,0,3,-1,439,0},
            new double[] {2,0,2,1,422,0},
            new double[] {2,0,-3,-1,421,0},
            new double[] {2,1,-1,1,-366,0},
            new double[] {2,1,0,1,-351,0},
            new double[] {4,0,0,1,331,0},
            new double[] {2,-1,1,1,315,0},
            new double[] {2,-2,0,-1,302,0},
            new double[] {0,0,1,3,-283,0},
            new double[] {2,1,1,-1,-229,0},
            new double[] {1,1,0,-1,223,0},
            new double[] {1,1,0,1,223,0},
            new double[] {0,1,-2,-1,-220,0},
            new double[] {2,1,-1,-1,-220,0},
            new double[] {1,0,1,1,-185,0},
            new double[] {2,-1,-2,-1,181,0},
            new double[] {0,1,2,1,-177,0},
            new double[] {4,0,-2,-1,176,0},
            new double[] {4,-1,-1,-1,166,0},
            new double[] {1,0,1,-1,-164,0},
            new double[] {4,0,1,-1,132,0},
            new double[] {1,0,-1,-1,-119,0},
            new double[] {4,-1,0,-1,115,0},
            new double[] {2,-2,0,1,107,0}
        };

        //-----------------Intermediate MPA OUTPUT VALUES--------------------   
        double l_prime;		//moon mean longitude [degrees]
        double d;			//moon mean elongation [degrees]
        double m;			//sun mean anomaly [degrees]
        double m_prime;		//moon mean anomaly [degrees]
        double f;           //moon argument of latitude [degrees]
        double l;			//term l
        double r;			//term r
        double b;			//term b
        double lamda_prime; //moon longitude [degrees]
        double beta;		//moon latitude [degrees]
        public double cap_delta;   //distance from earth to moon [kilometers]
        public double pi;          //moon equatorial horizontal parallax [degrees] 
        double lamda;       //apparent moon longitude [degrees]
        double alpha;       //geocentric moon right ascension [degrees]
        double delta;       //geocentric moon declination [degrees]
        double h;           //observer hour angle [degrees]
        double del_alpha;   //moon right ascension parallax [degrees]
        double delta_prime; //topocentric moon declination [degrees]
        double alpha_prime; //topocentric moon right ascension [degrees]
        double h_prime;     //topocentric local hour angle [degrees]
        double e0;          //topocentric elevation angle (uncorrected) [degrees]
        double del_e;       //atmospheric refraction correction [degrees]
        public double e;           //topocentric elevation angle (corrected) [degrees]
        //---------------------Final MPA OUTPUT VALUES------------------------
        public double zenith;        //topocentric zenith angle [degrees]
        double azimuth_astro;        //topocentric azimuth angle (westward from south) [for astronomers]
        public double azimuth;       //topocentric azimuth angle (eastward from north) [for navigators and solar radiation]

        public void Calculate(Sampa sampa, Spa spa)
        {
            l_prime = moon_mean_longitude(spa.jce);
            d = moon_mean_elongation(spa.jce);
            m = sun_mean_anomaly(spa.jce);
            m_prime = moon_mean_anomaly(spa.jce);
            f = moon_latitude_argument(spa.jce);

            moon_periodic_term_summation(d, m, m_prime, f, spa.jce, ML_TERMS, ref l, ref r);
            double unused = 0;
            moon_periodic_term_summation(d, m, m_prime, f, spa.jce, MB_TERMS, ref b, ref unused);

            moon_longitude_and_latitude(spa.jce, l_prime, f, m_prime, l, b);

            cap_delta = moon_earth_distance(r);
            pi = moon_equatorial_horiz_parallax(cap_delta);

            lamda = apparent_moon_longitude(lamda_prime, spa.del_psi);

            alpha = Spa.geocentric_right_ascension(lamda, spa.epsilon, beta);
            delta = Spa.geocentric_declination(beta, spa.epsilon, lamda);

            h  = Spa.observer_hour_angle(spa.nu, sampa.longitude, alpha);

            Spa.right_ascension_parallax_and_topocentric_dec(sampa.latitude, sampa.elevation, pi, h, delta, ref del_alpha, ref delta_prime);
            alpha_prime = Spa.topocentric_right_ascension(alpha, del_alpha);
            h_prime = Spa.topocentric_local_hour_angle(h, del_alpha);

            e0 = Spa.topocentric_elevation_angle(sampa.latitude, delta_prime, h_prime);
            del_e = Spa.atmospheric_refraction_correction(sampa.pressure, sampa.temperature, sampa.atmos_refract, e0);
            e = Spa.topocentric_elevation_angle_corrected(e0, del_e);

            zenith = Spa.topocentric_zenith_angle(e);
            azimuth_astro = Spa.topocentric_azimuth_angle_astro(h_prime, sampa.latitude, delta_prime);
            azimuth = Spa.topocentric_azimuth_angle(azimuth_astro);
        }

        double moon_mean_longitude(double jce)
        {
            return Spa.limit_degrees(fourth_order_polynomial(
                                -1.0/65194000, 1.0/538841, -0.0015786, 481267.88123421, 218.3164477, jce));
        }

        double moon_mean_elongation(double jce)
        {
            return Spa.limit_degrees(fourth_order_polynomial(
                                -1.0/113065000, 1.0/545868, -0.0018819, 445267.1114034, 297.8501921, jce));
        }

        double sun_mean_anomaly(double jce)
        {
            return Spa.limit_degrees(Spa.third_order_polynomial(
                                1.0/24490000, -0.0001536, 35999.0502909, 357.5291092, jce));
        }

        double moon_mean_anomaly(double jce)
        {
            return Spa.limit_degrees(fourth_order_polynomial(
                                -1.0/14712000, 1.0/69699, 0.0087414, 477198.8675055, 134.9633964, jce));
        }

        double moon_latitude_argument(double jce)
        {
            return Spa.limit_degrees(fourth_order_polynomial(
                                1.0/863310000, -1.0/3526000, -0.0036539, 483202.0175233, 93.2720950, jce));
        }

        void moon_periodic_term_summation(double d, double m, double m_prime, double f, double jce, double[][] terms, ref double sin_sum, ref double cos_sum)
        {
            int i;
            double e_mult, trig_arg;
            double e = 1.0 - jce*(0.002516 + jce*0.0000074);

            sin_sum = 0;
            if (cos_sum != 0) cos_sum = 0;

            for (i = 0; i < COUNT; i++)
            {
                e_mult = System.Math.Pow(e, System.Math.Abs(terms[i][(int) TERM.TERM_M]));
                trig_arg = Spa.deg2rad(terms[i][(int) TERM.TERM_D]*d + terms[i][(int) TERM.TERM_M]  *m +
                                terms[i][(int) TERM.TERM_F]*f + terms[i][(int) TERM.TERM_MPR]*m_prime);
                sin_sum += e_mult * terms[i][(int) TERM.TERM_LB] *System.Math.Sin(trig_arg);
                if (cos_sum != 0)  cos_sum += e_mult * terms[i][(int) TERM.TERM_R]  * System.Math.Cos(trig_arg);
            }
        }

        void moon_longitude_and_latitude(double jce, double l_prime, double f, double m_prime, double l, double b)
        {
            double a1 = 119.75 + 131.849*jce;
            double a2 = 53.09 + 479264.290*jce;
            double a3 = 313.45 + 481266.484*jce;
            double delta_l = 3958*System.Math.Sin(Spa.deg2rad(a1)) + 318*System.Math.Sin(Spa.deg2rad(a2)) + 1962*System.Math.Sin(Spa.deg2rad(l_prime-f));
            double delta_b = -2235*System.Math.Sin(Spa.deg2rad(l_prime)) + 175*System.Math.Sin(Spa.deg2rad(a1-f)) + 127*System.Math.Sin(Spa.deg2rad(l_prime-m_prime))
                            + 382*System.Math.Sin(Spa.deg2rad(a3)) + 175*System.Math.Sin(Spa.deg2rad(a1+f)) - 115*System.Math.Sin(Spa.deg2rad(l_prime+m_prime));

            lamda_prime = Spa.limit_degrees(l_prime + (l + delta_l)/1000000);
            beta = Spa.limit_degrees((b + delta_b)/1000000);
        }

        double moon_earth_distance(double r)
        {
            return 385000.56 + r/1000;
        }

        double moon_equatorial_horiz_parallax(double delta)
        {
            return Spa.rad2deg(System.Math.Asin(6378.14/delta));
        }

        double apparent_moon_longitude(double lamda_prime, double del_psi)
        {
            return lamda_prime + del_psi;
        }

        double fourth_order_polynomial(double a, double b, double c, double d, double e, double x)
        {
            return (((a*x + b)*x + c)*x + d)*x + e;
        }
    }
}