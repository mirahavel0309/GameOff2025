using UnityEngine;



public class GameBoard : MonoBehaviour
{
    public Transform playerEnterLocation;
    public Transform[] playerLocations;

    public Transform enemyEnterLocation;
    public Transform[] enemyLocations;

    public Transform[] exitPath;
    public CameraPathPoint[] exitCameraPath;
    public CameraPathPoint[] enterCameraPath;
}
