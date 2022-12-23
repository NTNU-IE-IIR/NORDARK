using System.Collections.Generic;

public class GroundTexture
{
    public int Texture;
    public List<Vector2d> Coordinates;

    public GroundTexture(int texture)
    {
        Texture = texture;
        Coordinates = new List<Vector2d>();
    }
}
