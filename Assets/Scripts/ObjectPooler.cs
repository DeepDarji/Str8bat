using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject pokemonPrefab;  // Assign the Pokémon ListItem prefab in the Inspector
    public int poolSize = 20;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(pokemonPrefab);
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (availableObjects.Count > 0)
        {
            GameObject obj = availableObjects.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        availableObjects.Enqueue(obj);
    }
}
