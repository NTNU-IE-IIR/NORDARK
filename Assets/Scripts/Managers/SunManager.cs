//https://courses.cs.duke.edu/fall01/cps124/resources/p91-preetham.pdf

using UnityEngine;
using UnityEngine.Assertions;

public class SunManager : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;
    private Light sun;
    private System.DateTime sceneDateTime;

    void Awake()
    {
        Assert.IsNotNull(mapManager);

        sun = GetComponent<Light>();

        sceneDateTime = System.DateTime.Now;
    }

    public System.DateTime GetCurrentDateTime()
    {
        return sceneDateTime;
    }

    public void SetCurrentDateTime(System.DateTime newDateTime)
    {
        sceneDateTime = newDateTime;
        transform.eulerAngles = CalculateSunPosition(mapManager.GetCurrentLocationCoordinates());
    }

    private Vector3 CalculateSunPosition(Vector2d latLon)
    {
        sceneDateTime = System.TimeZoneInfo.ConvertTimeToUtc(sceneDateTime);

        // Number of days from J2000.0.  
        float julianDate = 367f * sceneDateTime.Year -
            (int)((7.0f / 4.0f) * (sceneDateTime.Year +
            (int)((sceneDateTime.Month + 9.0f) / 12.0f))) +
            (int)((275.0f * sceneDateTime.Month) / 9.0f) +
            sceneDateTime.Day - 730531.5f;

        float julianCenturies = julianDate / 36525.0f;

        // Sidereal Time  
        float siderealTimeHours = 6.6974f + 2400.0513f * julianCenturies;
        float siderealTimeUT = siderealTimeHours + (366.2422f / 365.2422f) * (float) sceneDateTime.TimeOfDay.TotalHours;
        float siderealTime = siderealTimeUT * 15f + (float) latLon.y;

        // Refine to number of days (fractional) to specific time.  
        julianDate += (float) sceneDateTime.TimeOfDay.TotalHours / 24.0f;
        julianCenturies = julianDate / 36525.0f;

        // Solar Coordinates  
        float meanLongitude = CorrectAngle(Mathf.Deg2Rad * (280.466f + 36000.77f * julianCenturies));
        float meanAnomaly = CorrectAngle(Mathf.Deg2Rad *(357.529f + 35999.05f * julianCenturies));
        float equationOfCenter = Mathf.Deg2Rad * ((1.915f - 0.005f * julianCenturies) * Mathf.Sin(meanAnomaly) + 0.02f * Mathf.Sin(2 * meanAnomaly));
        float elipticalLongitude = CorrectAngle(meanLongitude + equationOfCenter);
        float obliquity = (23.439f - 0.013f * julianCenturies) * Mathf.Deg2Rad;

        // Right Ascension  
        float rightAscension = Mathf.Atan2(Mathf.Cos(obliquity) * Mathf.Sin(elipticalLongitude), Mathf.Cos(elipticalLongitude));

        float declination = Mathf.Asin(Mathf.Sin(rightAscension) * Mathf.Sin(obliquity));

        // Horizontal Coordinates  
        float hourAngle = CorrectAngle(siderealTime * Mathf.Deg2Rad) - rightAscension;

        if (hourAngle > Mathf.PI) {
            hourAngle -= 2 * Mathf.PI;
        }

        float altitude = Mathf.Asin(Mathf.Sin((float) latLon.x * Mathf.Deg2Rad) *
            Mathf.Sin(declination) + Mathf.Cos((float) latLon.x * Mathf.Deg2Rad) *
            Mathf.Cos(declination) * Mathf.Cos(hourAngle));

        // Nominator and denominator for calculating Azimuth  
        // angle. Needed to test which quadrant the angle is in.  
        float aziNom = -Mathf.Sin(hourAngle);
        float aziDenom =
            Mathf.Tan(declination) * Mathf.Cos((float) latLon.x * Mathf.Deg2Rad) -
            Mathf.Sin((float) latLon.x * Mathf.Deg2Rad) * Mathf.Cos(hourAngle);

        float azimuth = Mathf.Atan(aziNom / aziDenom);

        if (aziDenom < 0) {     // In 2nd or 3rd quadrant
            azimuth += Mathf.PI;
        }
        else if (aziNom < 0) {  // In 4th quadrant  
            azimuth += 2 * Mathf.PI;
        }

        return new Vector3(altitude * Mathf.Rad2Deg, azimuth * Mathf.Rad2Deg, 0);
    }

    private float CorrectAngle(float angleInRadians)
    {
        if (angleInRadians < 0) {
            return 2 * Mathf.PI - (Mathf.Abs(angleInRadians) % (2f * Mathf.PI));
        } else if (angleInRadians > 2f * Mathf.PI) {
            return angleInRadians % (2f * Mathf.PI);
        } else {
            return angleInRadians;
        }
    }
}