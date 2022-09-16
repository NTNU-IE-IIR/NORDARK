public class LightIntensity
{
    public float Value { get; set; }
    public UnityEngine.Rendering.HighDefinition.LightUnit Unit  { get; set; }

    public LightIntensity(float value, UnityEngine.Rendering.HighDefinition.LightUnit unit)
    {
        Value = value;
        Unit = unit;
    }
}