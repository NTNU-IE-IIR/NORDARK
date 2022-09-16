public class IESLight
{
    public string Name { get; set; }
    public UnityEngine.Texture Cookie { get; set; }
    public LightIntensity Intensity { get; set; }

    public IESLight(string name, UnityEngine.Texture cookie, LightIntensity intensity)
    {
        Name = name;
        Cookie = cookie;
        Intensity = intensity;
    }
}