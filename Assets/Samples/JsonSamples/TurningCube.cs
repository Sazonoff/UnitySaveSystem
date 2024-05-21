using UnityEngine;

namespace UnitySaveSystem.Saves.Samples
{
    public class TurningCube : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(Vector3.forward, Time.deltaTime * 100);
        }
    }
}