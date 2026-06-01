using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Inst { get; private set; }

    [Header("타워 프리팹")]
    public GameObject[] towerPrefabs;

    [Header("배치 설정")]
    public LayerMask placementLayer;
    public LayerMask towerLayer;

    [Header("타워 비용")]
    public int[] towerCosts = { 100, 150, 200, 250, 300 };

    private GameObject _previewTower;
    private int _selectedIndex = -1;
    private bool _isPlacing = false;

    private void Awake()
    {
        Inst = this;
    }

    private void Update()
    {
        if (!_isPlacing) return;

        Vector3 mousePos = GetMouseWorldPos();

        if (_previewTower != null)
            _previewTower.transform.position = mousePos;

        // 좌클릭 → 배치
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            TryPlaceTower(mousePos);
        }

        // 우클릭 → 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void SelectTower(int index)
    {
        if (!GoldManager.Inst.HasGold(towerCosts[index]))
        {
            Debug.Log("골드가 부족합니다!");
            return;
        }

        if (_previewTower != null)
            Destroy(_previewTower);

        _selectedIndex = index;
        _isPlacing = true;

        _previewTower = Instantiate(towerPrefabs[index]);
        SetPreviewAlpha(_previewTower, 0.5f);
    }

    private void TryPlaceTower(Vector3 pos)
    {
        // 배치 가능 구역 확인
        Collider2D hit = Physics2D.OverlapPoint(pos, placementLayer);
        if (hit == null)
        {
            Debug.Log("배치 불가능한 위치!");
            return;
        }

        // 이미 타워 있는지 확인
        Collider2D existing = Physics2D.OverlapPoint(pos, towerLayer);
        if (existing != null)
        {
            Debug.Log("이미 타워가 있습니다!");
            return;
        }

        // 골드 차감
        GoldManager.Inst.SpendGold(towerCosts[_selectedIndex]);

        // 타워 배치
        Destroy(_previewTower);
        GameObject tower = Instantiate(
            towerPrefabs[_selectedIndex], 
            new Vector3(pos.x, pos.y, 0), 
            Quaternion.identity
        );
        tower.layer = LayerMask.NameToLayer("Tower");

        _isPlacing = false;
        _selectedIndex = -1;
        _previewTower = null;
    }

    private void CancelPlacement()
    {
        if (_previewTower != null)
            Destroy(_previewTower);

        _isPlacing = false;
        _selectedIndex = -1;
        _previewTower = null;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void SetPreviewAlpha(GameObject obj, float alpha)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }
}