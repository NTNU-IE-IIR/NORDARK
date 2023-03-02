using UnityEngine;

public interface IComputationObject
{
    public const float EXTREMUM_DEFAULT_VALUE = Mathf.Infinity;
    public void Show(bool show);
    public void Draw();
    public void Erase();
    public void ImportResults();
    public void ExportResultsGeoJSON();
    public void ExportResultsCSV();
    public void DisplayValues(bool display);
    public void ShowVisualizationMethod(bool show);
    public void GetPositionsAnglesAlongObject(out Vector3[] positions, out float[] angles);
    public void ResultsComputed(Vector3[] positions, float[,] luminances);
}
