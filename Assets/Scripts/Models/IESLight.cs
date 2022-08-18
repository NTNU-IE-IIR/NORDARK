public class IESLight
{
    public string Name { get; set; }
    public UnityEngine.Texture Cookie { get; set; }

    public IESLight(string name, UnityEngine.Texture cookie)
    {
        Name = name;
        Cookie = cookie;
    }
}