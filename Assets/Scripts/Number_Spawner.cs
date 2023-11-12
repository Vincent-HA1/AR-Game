using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Number_Spawner : MonoBehaviour
{
    // This class handles the spawning of the numbers 1-10
    private Bounds numberPositionRange; //stores the bounds of the collider passed in as positionCollider. Used to decide location of numbers
    private List<GameObject> spawnedNumbers =  new List<GameObject>(); //stores the gameobjects that have been spawned so that they can be destroyed at a later time
    public List<GameObject> allNumberPrefabs; //stores all the number prefabs 1-10
    [SerializeField] BoxCollider positionCollider; //the collider which marks the locations the numbers can spawn from
    [SerializeField] Transform numbersParent; //a gameobject to store the numbers under
    // Start is called before the first frame update
    void Start()
    {
        numberPositionRange = positionCollider.bounds; //assign the bounds
    }

    /* 
     * A Function to Spawn a given number prefab at a set x position. If none is given, it spawns the number at a random position in the bounds
     * Conveniently, the number can tell us what index of number to spawn. E.g. number 1 will be at index 0 (as it is the first number in the list).
    */
    public void Spawn(int number, float xOffset = -1000)
    {
        xOffset = xOffset == -1000 ? Random.Range(numberPositionRange.min.x, numberPositionRange.max.x) : xOffset;
        Vector3 randomPosition = new Vector3(xOffset, Random.Range(numberPositionRange.min.y, numberPositionRange.max.y), Random.Range(numberPositionRange.min.z, numberPositionRange.max.z));
        //spawn at random position around 
        spawnedNumbers.Add(Instantiate(allNumberPrefabs[number - 1], randomPosition, allNumberPrefabs[number - 1].transform.rotation, numbersParent));
    }

    /*
     * This function runs the spawn function multiple times on a list of numbers and spawns each one
     * It also spawns each one at an offset so that they don't start in the same position
    */

    public void SpawnListOfNumbers(List<int> numbersToSpawn)
    {
        float xOffset = (numberPositionRange.max.x - numberPositionRange.min.x) / numbersToSpawn.Count;
        for (int i= 0; i < numbersToSpawn.Count; i++)
        {
            Spawn(numbersToSpawn[i], numberPositionRange.min.x + xOffset * i);
            spawnedNumbers[spawnedNumbers.Count - 1].GetComponent<Number>().ShouldBounce(); //these numbers should be bouncing around
        }
    }

    /*
     * This function destroys all the numbers in the spawned Numbers list and clears it
    */
    public void DestroyAllNumbers()
    {
        for(int i = 0; i<spawnedNumbers.Count; i++)
        {
            Destroy(spawnedNumbers[i]);
        }
        spawnedNumbers.Clear();
    }

}
