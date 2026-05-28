using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public static Transform[] points;

    void Awake()
    {
        // 자식으로 등록된 이정표들의 개수만큼 배열 크기를 지정합니다.
        points = new Transform[transform.childCount];

        // 자식 오브젝트들의 위치(Transform) 정보들을 순서대로 배열에 담습니다.
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }
}