using UnityEngine;

public class StreetViewManager : MonoBehaviour
{
  public GameObject mainCamera;
  public GameObject FPSCharacter;
  
  public void DisplayCameraPreview(bool shouldDisplay)
  {
    if(shouldDisplay){
      mainCamera.SetActive(false);
      FPSCharacter.SetActive(true);
    } else {
      mainCamera.SetActive(true);
      FPSCharacter.SetActive(false);
    }
  }

  public void ChangePreset(string value)
  {
    Debug.Log("ChangePreset: " + value);
  }
}
