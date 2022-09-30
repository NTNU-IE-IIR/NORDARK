public class IESLight
{
    public string Name { get; set; }
    public UnityEngine.Cubemap Cookie { get; set; }
    public LightIntensity Intensity { get; set; }

    public IESLight(string name, UnityEngine.Cubemap cookie, LightIntensity intensity)
    {
        Name = name;
        Cookie = cookie;
        Intensity = intensity;
    }
}