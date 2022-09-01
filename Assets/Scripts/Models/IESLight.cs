public class IESLight
{
    public string Name { get; set; }
    public UnityEngine.Texture Cookie { get; set; }
    public float Intensity { get; set; }

    public IESLight(string name, UnityEngine.Texture cookie, float intensity)
    {
        Name = name;
        Cookie = cookie;
        Intensity = intensity;
    }
}