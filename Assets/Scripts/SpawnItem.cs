using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public Transform spawnPoint;
    public List<GameObject> cubes;

    public void Spawn (GameObject Item)
    {
        GameObject itemToSpawn = Instantiate(Item, spawnPoint.position, spawnPoint.rotation);
    }

    public void SpawnCube()
    {
        int randCube = Random.Range(0, cubes.Count);

        Spawn(cubes[randCube]);
    }
}
