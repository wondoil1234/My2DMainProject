using System.Collections.Generic;
using UnityEngine;

public class LifeContainer : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab; 
    private List<GameObject> _heartList = new List<GameObject>();

    public void InitHearts(int maxLife)
    {
        foreach (var heart in _heartList)
            Destroy(heart);
        _heartList.Clear();

        for (int i = 0; i < maxLife; i++)
        {
            var heart = Instantiate(heartPrefab, transform);
            _heartList.Add(heart);
        }
    }

    public void UpdateHearts(int currentLife)
    {
        for (int i = 0; i < _heartList.Count; i++)
        {
            _heartList[i].SetActive(i < currentLife);
        }
    }
}