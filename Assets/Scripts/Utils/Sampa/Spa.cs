namespace Sampa {
    public class Spa
    {
        const double SUN_RADIUS = 0.26667;
        const int L_COUNT = 6;
        const int B_COUNT = 2;
        const int R_COUNT = 5;
        const int Y_COUNT = 63;

        enum TERM {TERM_A, TERM_B, TERM_C, TERM_COUNT};
        enum TERM_X {TERM_X0, TERM_X1, TERM_X2, TERM_X3, TERM_X4, TERM_X_COUNT};
        enum TERM_PSI {TERM_PSI_A, TERM_PSI_B, TERM_EPS_C, TERM_EPS_D, TERM_PE_COUNT};

        int[] l_subcount = new int[]{64,34,20,7,3,1};
        int[] b_subcount = new int[]{5,2};
        int[] r_subcount = new int[]{40,10,6,2,1};

        double[][][] L_TERMS = new double[][][] {
            new double[][] {
                new double[] {175347046.0,0,0},
                new double[] {3341656.0,4.6692568,6283.07585},
                new double[] {34894.0,4.6261,12566.1517},
                new double[] {3497.0,2.7441,5753.3849},
                new double[] {3418.0,2.8289,3.5231},
                new double[] {3136.0,3.6277,77713.7715},
                new double[] {2676.0,4.4181,7860.4194},
                new double[] {2343.0,6.1352,3930.2097},
                new double[] {1324.0,0.7425,11506.7698},
                new double[] {1273.0,2.0371,529.691},
                new double[] {1199.0,1.1096,1577.3435},
                new double[] {990,5.233,5884.927},
                new double[] {902,2.045,26.298},
                new double[] {857,3.508,398.149},
                new double[] {780,1.179,5223.694},
                new double[] {753,2.533,5507.553},
                new double[] {505,4.583,18849.228},
                new double[] {492,4.205,775.523},
                new double[] {357,2.92,0.067},
                new double[] {317,5.849,11790.629},
                new double[] {284,1.899,796.298},
                new double[] {271,0.315,10977.079},
                new double[] {243,0.345,5486.778},
                new double[] {206,4.806,2544.314},
                new double[] {205,1.869,5573.143},
                new double[] {202,2.458,6069.777},
                new double[] {156,0.833,213.299},
                new double[] {132,3.411,2942.463},
                new double[] {126,1.083,20.775},
                new double[] {115,0.645,0.98},
                new double[] {103,0.636,4694.003},
                new double[] {102,0.976,15720.839},
                new double[] {102,4.267,7.114},
                new double[] {99,6.21,2146.17},
                new double[] {98,0.68,155.42},
                new double[] {86,5.98,161000.69},
                new double[] {85,1.3,6275.96},
                new double[] {85,3.67,71430.7},
                new double[] {80,1.81,17260.15},
                new double[] {79,3.04,12036.46},
                new double[] {75,1.76,5088.63},
                new double[] {74,3.5,3154.69},
                new double[] {74,4.68,801.82},
                new double[] {70,0.83,9437.76},
                new double[] {62,3.98,8827.39},
                new double[] {61,1.82,7084.9},
                new double[] {57,2.78,6286.6},
                new double[] {56,4.39,14143.5},
                new double[] {56,3.47,6279.55},
                new double[] {52,0.19,12139.55},
                new double[] {52,1.33,1748.02},
                new double[] {51,0.28,5856.48},
                new double[] {49,0.49,1194.45},
                new double[] {41,5.37,8429.24},
                new double[] {41,2.4,19651.05},
                new double[] {39,6.17,10447.39},
                new double[] {37,6.04,10213.29},
                new double[] {37,2.57,1059.38},
                new double[] {36,1.71,2352.87},
                new double[] {36,1.78,6812.77},
                new double[] {33,0.59,17789.85},
                new double[] {30,0.44,83996.85},
                new double[] {30,2.74,1349.87},
                new double[] {25,3.16,4690.48}
            },
            new double[][] {
                new double[] {628331966747.0,0,0},
                new double[] {206059.0,2.678235,6283.07585},
                new double[] {4303.0,2.6351,12566.1517},
                new double[] {425.0,1.59,3.523},
                new double[] {119.0,5.796,26.298},
                new double[] {109.0,2.966,1577.344},
                new double[] {93,2.59,18849.23},
                new double[] {72,1.14,529.69},
                new double[] {68,1.87,398.15},
                new double[] {67,4.41,5507.55},
                new double[] {59,2.89,5223.69},
                new double[] {56,2.17,155.42},
                new double[] {45,0.4,796.3},
                new double[] {36,0.47,775.52},
                new double[] {29,2.65,7.11},
                new double[] {21,5.34,0.98},
                new double[] {19,1.85,5486.78},
                new double[] {19,4.97,213.3},
                new double[] {17,2.99,6275.96},
                new double[] {16,0.03,2544.31},
                new double[] {16,1.43,2146.17},
                new double[] {15,1.21,10977.08},
                new double[] {12,2.83,1748.02},
                new double[] {12,3.26,5088.63},
                new double[] {12,5.27,1194.45},
                new double[] {12,2.08,4694},
                new double[] {11,0.77,553.57},
                new double[] {10,1.3,6286.6},
                new double[] {10,4.24,1349.87},
                new double[] {9,2.7,242.73},
                new double[] {9,5.64,951.72},
                new double[] {8,5.3,2352.87},
                new double[] {6,2.65,9437.76},
                new double[] {6,4.67,4690.48}
            },
            new double[][] {
                new double[] {52919.0,0,0},
                new double[] {8720.0,1.0721,6283.0758},
                new double[] {309.0,0.867,12566.152},
                new double[] {27,0.05,3.52},
                new double[] {16,5.19,26.3},
                new double[] {16,3.68,155.42},
                new double[] {10,0.76,18849.23},
                new double[] {9,2.06,77713.77},
                new double[] {7,0.83,775.52},
                new double[] {5,4.66,1577.34},
                new double[] {4,1.03,7.11},
                new double[] {4,3.44,5573.14},
                new double[] {3,5.14,796.3},
                new double[] {3,6.05,5507.55},
                new double[] {3,1.19,242.73},
                new double[] {3,6.12,529.69},
                new double[] {3,0.31,398.15},
                new double[] {3,2.28,553.57},
                new double[] {2,4.38,5223.69},
                new double[] {2,3.75,0.98}
            },
            new double[][] {
                new double[] {289.0,5.844,6283.076},
                new double[] {35,0,0},
                new double[] {17,5.49,12566.15},
                new double[] {3,5.2,155.42},
                new double[] {1,4.72,3.52},
                new double[] {1,5.3,18849.23},
                new double[] {1,5.97,242.73}
            },
            new double[][] {
                new double[] {114.0,3.142,0},
                new double[] {8,4.13,6283.08},
                new double[] {1,3.84,12566.15}
            },
            new double[][] {
                new double[] {1,3.14,0}
            }
        };

        double[][][] B_TERMS = new double[][][] {
            new double[][] {
                new double[] {280.0,3.199,84334.662},
                new double[] {102.0,5.422,5507.553},
                new double[] {80,3.88,5223.69},
                new double[] {44,3.7,2352.87},
                new double[] {32,4,1577.34}
            },
            new double[][] {
                new double[] {9,3.9,5507.55},
                new double[] {6,1.73,5223.69}
            }
        };

        double[][][] R_TERMS = new double[][][] {
            new double[][] {
                new double[] {100013989.0,0,0},
                new double[] {1670700.0,3.0984635,6283.07585},
                new double[] {13956.0,3.05525,12566.1517},
                new double[] {3084.0,5.1985,77713.7715},
                new double[] {1628.0,1.1739,5753.3849},
                new double[] {1576.0,2.8469,7860.4194},
                new double[] {925.0,5.453,11506.77},
                new double[] {542.0,4.564,3930.21},
                new double[] {472.0,3.661,5884.927},
                new double[] {346.0,0.964,5507.553},
                new double[] {329.0,5.9,5223.694},
                new double[] {307.0,0.299,5573.143},
                new double[] {243.0,4.273,11790.629},
                new double[] {212.0,5.847,1577.344},
                new double[] {186.0,5.022,10977.079},
                new double[] {175.0,3.012,18849.228},
                new double[] {110.0,5.055,5486.778},
                new double[] {98,0.89,6069.78},
                new double[] {86,5.69,15720.84},
                new double[] {86,1.27,161000.69},
                new double[] {65,0.27,17260.15},
                new double[] {63,0.92,529.69},
                new double[] {57,2.01,83996.85},
                new double[] {56,5.24,71430.7},
                new double[] {49,3.25,2544.31},
                new double[] {47,2.58,775.52},
                new double[] {45,5.54,9437.76},
                new double[] {43,6.01,6275.96},
                new double[] {39,5.36,4694},
                new double[] {38,2.39,8827.39},
                new double[] {37,0.83,19651.05},
                new double[] {37,4.9,12139.55},
                new double[] {36,1.67,12036.46},
                new double[] {35,1.84,2942.46},
                new double[] {33,0.24,7084.9},
                new double[] {32,0.18,5088.63},
                new double[] {32,1.78,398.15},
                new double[] {28,1.21,6286.6},
                new double[] {28,1.9,6279.55},
                new double[] {26,4.59,10447.39}
            },
            new double[][] {
                new double[] {103019.0,1.10749,6283.07585},
                new double[] {1721.0,1.0644,12566.1517},
                new double[] {702.0,3.142,0},
                new double[] {32,1.02,18849.23},
                new double[] {31,2.84,5507.55},
                new double[] {25,1.32,5223.69},
                new double[] {18,1.42,1577.34},
                new double[] {10,5.91,10977.08},
                new double[] {9,1.42,6275.96},
                new double[] {9,0.27,5486.78}
            },
            new double[][] {
                new double[] {4359.0,5.7846,6283.0758},
                new double[] {124.0,5.579,12566.152},
                new double[] {12,3.14,0},
                new double[] {9,3.63,77713.77},
                new double[] {6,1.87,5573.14},
                new double[] {3,5.47,18849.23}
            },
            new double[][] {
                new double[] {145.0,4.273,6283.076},
                new double[] {7,3.92,12566.15}
            },
            new double[][] {
                new double[] {4,2.56,6283.08}
            }
        };

        int[][] Y_TERMS = new int[][] {
            new int[] {0,0,0,0,1},
            new int[] {-2,0,0,2,2},
            new int[] {0,0,0,2,2},
            new int[] {0,0,0,0,2},
            new int[] {0,1,0,0,0},
            new int[] {0,0,1,0,0},
            new int[] {-2,1,0,2,2},
            new int[] {0,0,0,2,1},
            new int[] {0,0,1,2,2},
            new int[] {-2,-1,0,2,2},
            new int[] {-2,0,1,0,0},
            new int[] {-2,0,0,2,1},
            new int[] {0,0,-1,2,2},
            new int[] {2,0,0,0,0},
            new int[] {0,0,1,0,1},
            new int[] {2,0,-1,2,2},
            new int[] {0,0,-1,0,1},
            new int[] {0,0,1,2,1},
            new int[] {-2,0,2,0,0},
            new int[] {0,0,-2,2,1},
            new int[] {2,0,0,2,2},
            new int[] {0,0,2,2,2},
            new int[] {0,0,2,0,0},
            new int[] {-2,0,1,2,2},
            new int[] {0,0,0,2,0},
            new int[] {-2,0,0,2,0},
            new int[] {0,0,-1,2,1},
            new int[] {0,2,0,0,0},
            new int[] {2,0,-1,0,1},
            new int[] {-2,2,0,2,2},
            new int[] {0,1,0,0,1},
            new int[] {-2,0,1,0,1},
            new int[] {0,-1,0,0,1},
            new int[] {0,0,2,-2,0},
            new int[] {2,0,-1,2,1},
            new int[] {2,0,1,2,2},
            new int[] {0,1,0,2,2},
            new int[] {-2,1,1,0,0},
            new int[] {0,-1,0,2,2},
            new int[] {2,0,0,2,1},
            new int[] {2,0,1,0,0},
            new int[] {-2,0,2,2,2},
            new int[] {-2,0,1,2,1},
            new int[] {2,0,-2,0,1},
            new int[] {2,0,0,0,1},
            new int[] {0,-1,1,0,0},
            new int[] {-2,-1,0,2,1},
            new int[] {-2,0,0,0,1},
            new int[] {0,0,2,2,1},
            new int[] {-2,0,2,0,1},
            new int[] {-2,1,0,2,1},
            new int[] {0,0,1,-2,0},
            new int[] {-1,0,1,0,0},
            new int[] {-2,1,0,0,0},
            new int[] {1,0,0,0,0},
            new int[] {0,0,1,2,0},
            new int[] {0,0,-2,2,2},
            new int[] {-1,-1,1,0,0},
            new int[] {0,1,1,0,0},
            new int[] {0,-1,1,2,2},
            new int[] {2,-1,-1,2,2},
            new int[] {0,0,3,2,2},
            new int[] {2,-1,0,2,2},
        };

        double[][] PE_TERMS = new double[][] {
            new double[] {-171996,-174.2,92025,8.9},
            new double[] {-13187,-1.6,5736,-3.1},
            new double[] {-2274,-0.2,977,-0.5},
            new double[] {2062,0.2,-895,0.5},
            new double[] {1426,-3.4,54,-0.1},
            new double[] {712,0.1,-7,0},
            new double[] {-517,1.2,224,-0.6},
            new double[] {-386,-0.4,200,0},
            new double[] {-301,0,129,-0.1},
            new double[] {217,-0.5,-95,0.3},
            new double[] {-158,0,0,0},
            new double[] {129,0.1,-70,0},
            new double[] {123,0,-53,0},
            new double[] {63,0,0,0},
            new double[] {63,0.1,-33,0},
            new double[] {-59,0,26,0},
            new double[] {-58,-0.1,32,0},
            new double[] {-51,0,27,0},
            new double[] {48,0,0,0},
            new double[] {46,0,-24,0},
            new double[] {-38,0,16,0},
            new double[] {-31,0,13,0},
            new double[] {29,0,0,0},
            new double[] {29,0,-12,0},
            new double[] {26,0,0,0},
            new double[] {-22,0,0,0},
            new double[] {21,0,-10,0},
            new double[] {17,-0.1,0,0},
            new double[] {16,0,-8,0},
            new double[] {-16,0.1,7,0},
            new double[] {-15,0,9,0},
            new double[] {-13,0,7,0},
            new double[] {-12,0,6,0},
            new double[] {11,0,0,0},
            new double[] {-10,0,5,0},
            new double[] {-8,0,3,0},
            new double[] {7,0,-3,0},
            new double[] {-7,0,0,0},
            new double[] {-7,0,3,0},
            new double[] {-7,0,3,0},
            new double[] {6,0,0,0},
            new double[] {6,0,-3,0},
            new double[] {6,0,-3,0},
            new double[] {-6,0,3,0},
            new double[] {-6,0,3,0},
            new double[] {5,0,0,0},
            new double[] {-5,0,3,0},
            new double[] {-5,0,3,0},
            new double[] {-5,0,3,0},
            new double[] {4,0,0,0},
            new double[] {4,0,0,0},
            new double[] {4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
        };

        //-----------------Intermediate OUTPUT VALUES--------------------
        public double jd;          //Julian day
        double jc;          //Julian century
        double jde;         //Julian ephemeris day
        public double jce;         //Julian ephemeris century
        double jme;         //Julian ephemeris millennium
        public double l;           //earth heliocentric longitude [degrees]
        public double b;           //earth heliocentric latitude [degrees]
        public double r;           //earth radius vector [Astronomical Units, AU]
        double theta;       //geocentric longitude [degrees]
        double beta;        //geocentric latitude [degrees]
        double x0;          //mean elongation (moon-sun) [degrees]
        double x1;          //mean anomaly (sun) [degrees]
        double x2;          //mean anomaly (moon) [degrees]
        double x3;          //argument latitude (moon) [degrees]
        double x4;          //ascending longitude (moon) [degrees]
        public double del_psi;     //nutation longitude [degrees]
        public double del_epsilon; //nutation obliquity [degrees]
        double epsilon0;    //ecliptic mean obliquity [arc seconds]
        public double epsilon;     //ecliptic true obliquity  [degrees]
        double del_tau;     //aberration correction [degrees]
        double lamda;       //apparent sun longitude [degrees]
        double nu0;         //Greenwich mean sidereal time [degrees]
        public double nu;          //Greenwich sidereal time [degrees]
        double alpha;       //geocentric sun right ascension [degrees]
        double delta;       //geocentric sun declination [degrees
        public double h;           //observer hour angle [degrees]
        double xi;          //sun equatorial horizontal parallax [degrees]
        double del_alpha;   //sun right ascension parallax [degrees]
        double delta_prime; //topocentric sun declination [degrees]
        double alpha_prime; //topocentric sun right ascension [degrees]
        double h_prime;     //topocentric local hour angle [degrees]
        double e0;          //topocentric elevation angle (uncorrected) [degrees]
        double del_e;       //atmospheric refraction correction [degrees]
        public double e;           //topocentric elevation angle (corrected) [degrees]

        //---------------------Final OUTPUT VALUES------------------------
        public double zenith;       //topocentric zenith angle [degrees]
        double azimuth_astro;//topocentric azimuth angle (westward from south) [for astronomers]
        public double azimuth;      //topocentric azimuth angle (eastward from north) [for navigators and solar radiation]

        public void Calculate(Sampa sampa)
        {
            jd = julian_day(sampa);

            calculate_geocentric_sun_right_ascension_and_declination(sampa.delta_t);

            h = observer_hour_angle(nu, sampa.longitude, alpha);
            xi = sun_equatorial_horizontal_parallax(r);

            right_ascension_parallax_and_topocentric_dec(sampa.latitude, sampa.elevation, xi, h, delta, ref del_alpha, ref delta_prime);

            alpha_prime = topocentric_right_ascension(alpha, del_alpha);
            h_prime = topocentric_local_hour_angle(h, del_alpha);

            e0 = topocentric_elevation_angle(sampa.latitude, delta_prime, h_prime);
            del_e = atmospheric_refraction_correction(sampa.pressure, sampa.temperature, sampa.atmos_refract, e0);
            e = topocentric_elevation_angle_corrected(e0, del_e);

            zenith = topocentric_zenith_angle(e);
            azimuth_astro = topocentric_azimuth_angle_astro(h_prime, sampa.latitude, delta_prime);
            azimuth = topocentric_azimuth_angle(azimuth_astro);
        }

        private double julian_day(Sampa sampa)
        {
            double day_decimal, julian_day, a;

            day_decimal = sampa.day + (sampa.hour - sampa.timezone + (sampa.minute + (sampa.second + sampa.delta_ut1)/60.0)/60.0)/24.0;

            if (sampa.month < 3) {
                sampa.month += 12;
                sampa.year--;
            }

            julian_day = (int)(365.25*(sampa.year+4716.0)) + (int)(30.6001*(sampa.month+1)) + day_decimal - 1524.5;

            if (julian_day > 2299160.0) {
                a = (int)(sampa.year/100);
                julian_day += (2 - a + (int)(a/4));
            }

            return julian_day;
        }

        void calculate_geocentric_sun_right_ascension_and_declination(double delta_t)
        {
            jc = julian_century(jd);

            jde = julian_ephemeris_day(jd, delta_t);
            jce = julian_ephemeris_century(jde);
            jme = julian_ephemeris_millennium(jce);

            l = earth_heliocentric_longitude(jme);
            b = earth_heliocentric_latitude(jme);
            r = earth_radius_vector(jme);

            theta = geocentric_longitude(l);
            beta  = geocentric_latitude(b);

            x0 = mean_elongation_moon_sun(jce);
            x1 = mean_anomaly_sun(jce);
            x2 = mean_anomaly_moon(jce);
            x3 = argument_latitude_moon(jce);
            x4 = ascending_longitude_moon(jce);

            double[] x = new double[] {x0, x1, x2, x3, x4};

            nutation_longitude_and_obliquity(jce, x);

            epsilon0 = ecliptic_mean_obliquity(jme);
            epsilon  = ecliptic_true_obliquity(del_epsilon, epsilon0);

            del_tau   = aberration_correction(r);
            lamda     = apparent_sun_longitude(theta, del_psi, del_tau);
            nu0       = greenwich_mean_sidereal_time (jd, jc);
            nu        = greenwich_sidereal_time (nu0, del_psi, epsilon);

            alpha = geocentric_right_ascension(lamda, epsilon, beta);
            delta = geocentric_declination(beta, epsilon, lamda);
        }

        public static double observer_hour_angle(double nu, double longitude, double alpha_deg)
        {
            return limit_degrees(nu + longitude - alpha_deg);
        }

        double sun_equatorial_horizontal_parallax(double r)
        {
            return 8.794 / (3600.0 * r);
        }

        public static void right_ascension_parallax_and_topocentric_dec(double latitude, double elevation, double xi, double h, double delta, ref double del_alpha, ref double delta_prime)
        {
            double delta_alpha_rad;
            double lat_rad   = deg2rad(latitude);
            double xi_rad    = deg2rad(xi);
            double h_rad     = deg2rad(h);
            double delta_rad = deg2rad(delta);
            double u = System.Math.Atan(0.99664719 * System.Math.Tan(lat_rad));
            double y = 0.99664719 * System.Math.Sin(u) + elevation*System.Math.Sin(lat_rad)/6378140.0;
            double x = System.Math.Cos(u) + elevation*System.Math.Cos(lat_rad)/6378140.0;
            delta_alpha_rad = System.Math.Atan2(- x*System.Math.Sin(xi_rad) *System.Math.Sin(h_rad),
                                                System.Math.Cos(delta_rad) - x*System.Math.Sin(xi_rad) *System.Math.Cos(h_rad));
            delta_prime = rad2deg(System.Math.Atan2((System.Math.Sin(delta_rad) - y*System.Math.Sin(xi_rad))*System.Math.Cos(delta_alpha_rad),
                                                    System.Math.Cos(delta_rad) - x*System.Math.Sin(xi_rad) *System.Math.Cos(h_rad)));

            del_alpha = rad2deg(delta_alpha_rad);
        }

        public static double topocentric_right_ascension(double alpha_deg, double delta_alpha)
        {
            return alpha_deg + delta_alpha;
        }

        public static double topocentric_local_hour_angle(double h, double delta_alpha)
        {
            return h - delta_alpha;
        }

        public static double topocentric_elevation_angle(double latitude, double delta_prime, double h_prime)
        {
            double lat_rad = deg2rad(latitude);
            double delta_prime_rad = deg2rad(delta_prime);

            return rad2deg(System.Math.Asin(System.Math.Sin(lat_rad)*System.Math.Sin(delta_prime_rad) +
                        System.Math.Cos(lat_rad)*System.Math.Cos(delta_prime_rad) * System.Math.Cos(deg2rad(h_prime))));
        }

        public static double atmospheric_refraction_correction(double pressure, double temperature, double atmos_refract, double e0)
        {
            double del_e = 0;

            if (e0 >= -1*(SUN_RADIUS + atmos_refract))
                del_e = (pressure / 1010.0) * (283.0 / (273.0 + temperature)) * 1.02 / (60.0 * System.Math.Tan(deg2rad(e0 + 10.3/(e0 + 5.11))));

            return del_e;
        }

        public static double topocentric_elevation_angle_corrected(double e0, double delta_e)
        {
            return e0 + delta_e;
        }

        public static double topocentric_zenith_angle(double e)
        {
            return 90.0 - e;
        }

        public static double topocentric_azimuth_angle_astro(double h_prime, double latitude, double delta_prime)
        {
            double h_prime_rad = deg2rad(h_prime);
            double lat_rad = deg2rad(latitude);

            return limit_degrees(rad2deg(System.Math.Atan2(System.Math.Sin(h_prime_rad),
                                System.Math.Cos(h_prime_rad)*System.Math.Sin(lat_rad) - System.Math.Tan(deg2rad(delta_prime))*System.Math.Cos(lat_rad))));
        }

        public static double topocentric_azimuth_angle(double azimuth_astro)
        {
            return limit_degrees(azimuth_astro + 180.0);
        }

        double julian_century(double jd)
        {
            return (jd-2451545.0)/36525.0;
        }

        double julian_ephemeris_day(double jd, double delta_t)
        {
            return jd+delta_t/86400.0;
        }

        double julian_ephemeris_century(double jde)
        {
            return (jde - 2451545.0)/36525.0;
        }

        double julian_ephemeris_millennium(double jce)
        {
            return (jce/10.0);
        }

        double earth_heliocentric_longitude(double jme)
        {
            double[] sum = new double[L_COUNT];
            int i;

            

            for (i = 0; i < L_COUNT; i++)
                sum[i] = earth_periodic_term_summation(L_TERMS[i], l_subcount[i], jme);

            return limit_degrees(rad2deg(earth_values(sum, L_COUNT, jme)));

        }

        double earth_heliocentric_latitude(double jme)
        {
            double[] sum = new double[B_COUNT];
            int i;

            for (i = 0; i < B_COUNT; i++)
                sum[i] = earth_periodic_term_summation(B_TERMS[i], b_subcount[i], jme);

            return rad2deg(earth_values(sum, B_COUNT, jme));

        }

        double earth_radius_vector(double jme)
        {
            double[] sum = new double[R_COUNT];
            int i;

            for (i = 0; i < R_COUNT; i++)
                sum[i] = earth_periodic_term_summation(R_TERMS[i], r_subcount[i], jme);

            return earth_values(sum, R_COUNT, jme);

        }

        double geocentric_longitude(double l)
        {
            double theta = l + 180.0;

            if (theta >= 360.0) theta -= 360.0;

            return theta;
        }

        double geocentric_latitude(double b)
        {
            return -b;
        }

        double mean_elongation_moon_sun(double jce)
        {
            return third_order_polynomial(1.0/189474.0, -0.0019142, 445267.11148, 297.85036, jce);
        }

        double mean_anomaly_sun(double jce)
        {
            return third_order_polynomial(-1.0/300000.0, -0.0001603, 35999.05034, 357.52772, jce);
        }

        double mean_anomaly_moon(double jce)
        {
            return third_order_polynomial(1.0/56250.0, 0.0086972, 477198.867398, 134.96298, jce);
        }

        double argument_latitude_moon(double jce)
        {
            return third_order_polynomial(1.0/327270.0, -0.0036825, 483202.017538, 93.27191, jce);
        }

        double ascending_longitude_moon(double jce)
        {
            return third_order_polynomial(1.0/450000.0, 0.0020708, -1934.136261, 125.04452, jce);
        }

        void nutation_longitude_and_obliquity(double jce, double[] x)
        {
            int i;
            double xy_term_sum, sum_psi=0, sum_epsilon=0;

            for (i = 0; i < Y_COUNT; i++) {
                xy_term_sum  = deg2rad(xy_term_summation(i, x));
                sum_psi     += (PE_TERMS[i][(int) TERM_PSI.TERM_PSI_A] + jce*PE_TERMS[i][(int) TERM_PSI.TERM_PSI_B])*System.Math.Sin(xy_term_sum);
                sum_epsilon += (PE_TERMS[i][(int) TERM_PSI.TERM_EPS_C] + jce*PE_TERMS[i][(int) TERM_PSI.TERM_EPS_D])*System.Math.Cos(xy_term_sum);
            }

            del_psi     = sum_psi     / 36000000.0;
            del_epsilon = sum_epsilon / 36000000.0;
        }

        double ecliptic_mean_obliquity(double jme)
        {
            double u = jme/10.0;

            return 84381.448 + u*(-4680.93 + u*(-1.55 + u*(1999.25 + u*(-51.38 + u*(-249.67 +
                            u*(  -39.05 + u*( 7.12 + u*(  27.87 + u*(  5.79 + u*2.45)))))))));
        }

        double ecliptic_true_obliquity(double delta_epsilon, double epsilon0)
        {
            return delta_epsilon + epsilon0/3600.0;
        }

        double aberration_correction(double r)
        {
            return -20.4898 / (3600.0*r);
        }

        double apparent_sun_longitude(double theta, double delta_psi, double delta_tau)
        {
            return theta + delta_psi + delta_tau;
        }

        double greenwich_mean_sidereal_time (double jd, double jc)
        {
            return limit_degrees(280.46061837 + 360.98564736629 * (jd - 2451545.0) +
                                            jc*jc*(0.000387933 - jc/38710000.0));
        }

        double greenwich_sidereal_time (double nu0, double delta_psi, double epsilon)
        {
            return nu0 + delta_psi*System.Math.Cos(deg2rad(epsilon));
        }

        public static double geocentric_right_ascension(double lamda, double epsilon, double beta)
        {
            double lamda_rad   = deg2rad(lamda);
            double epsilon_rad = deg2rad(epsilon);

            return limit_degrees(rad2deg(System.Math.Atan2(System.Math.Sin(lamda_rad)*System.Math.Cos(epsilon_rad) -
                                            System.Math.Tan(deg2rad(beta))*System.Math.Sin(epsilon_rad), System.Math.Cos(lamda_rad))));
        }

        public static double geocentric_declination(double beta, double epsilon, double lamda)
        {
            double beta_rad    = deg2rad(beta);
            double epsilon_rad = deg2rad(epsilon);

            return rad2deg(System.Math.Asin(System.Math.Sin(beta_rad)*System.Math.Cos(epsilon_rad) +
                                System.Math.Cos(beta_rad)*System.Math.Sin(epsilon_rad)*System.Math.Sin(deg2rad(lamda))));
        }

        double earth_periodic_term_summation(double [][]terms, int count, double jme)
        {
            int i;
            double sum=0;

            for (i = 0; i < count; i++)
                sum += terms[i][(int) TERM.TERM_A]*System.Math.Cos(terms[i][(int) TERM.TERM_B]+terms[i][(int) TERM.TERM_C]*jme);

            return sum;
        }

        public static double limit_degrees(double degrees)
        {
            double limited;

            degrees /= 360.0;
            limited = 360.0*(degrees-System.Math.Floor(degrees));
            if (limited < 0) limited += 360.0;

            return limited;
        }

        double earth_values(double[] term_sum, int count, double jme)
        {
            int i;
            double sum=0;

            for (i = 0; i < count; i++)
                sum += term_sum[i]*System.Math.Pow(jme, i);

            sum /= 1.0e8;

            return sum;
        }

        public static double rad2deg(double radians)
        {
            return (180.0/System.Math.PI)*radians;
        }

        public static double deg2rad(double degrees)
        {
            return (System.Math.PI/180.0)*degrees;
        }

        public static double third_order_polynomial(double a, double b, double c, double d, double x)
        {
            return ((a*x + b)*x + c)*x + d;
        }

        double xy_term_summation(int i, double[] x)
        {
            int j;
            double sum=0;

            for (j = 0; j < (int) TERM_X.TERM_X_COUNT; j++)
                sum += x[j]*Y_TERMS[i][j];

            return sum;
        }
    }
}