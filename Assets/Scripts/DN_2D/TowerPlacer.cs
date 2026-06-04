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

    [Header("버튼 텍스트 연결")]
    public UnityEngine.UI.Text[] buttonTexts;

    [Header("타워 이름")]
    public string[] towerNames = { "검정", "파랑", "보라", "빨강", "노랑" };

    private GameObject _previewTower;
    private int _selectedIndex = -1;
    private bool _isPlacing = false;

    private void Start()
    {
        UpdateButtonTexts();
    }

    private void UpdateButtonTexts()
    {
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] != null)
                buttonTexts[i].text = $"{towerNames[i]}\n{towerCosts[i]}G";
        }
    }

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

        _previewTower.layer = LayerMask.NameToLayer("Default");
        foreach (Transform child in _previewTower.GetComponentsInChildren<Transform>())
            child.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void StartPlacementFromShop(string towerDataId)
    {
        int targetIndex = -1;
        for (int i = 0; i < towerPrefabs.Length; i++)
        {
            if (towerPrefabs[i] != null && towerPrefabs[i].name == towerDataId)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            Debug.LogError($"[TowerPlacer] 프리팹 배열에서 '{towerDataId}' 이름을 찾을 수 없습니다! 인스펙터 설정을 확인하세요.");
            return;
        }

        SelectTower(targetIndex);
    }

    private void TryPlaceTower(Vector3 pos)
    {
        Vector2 pos2D = new Vector2(pos.x, pos.y);

        if (pos.y >= -2f && pos.y <= 2f)
        {
            Debug.Log("몬스터 경로입니다!");
            return;
        }

        Collider2D hit = Physics2D.OverlapPoint(pos2D, placementLayer);
        if (hit == null)
        {
            Debug.Log("배치 불가능한 위치!");
            return;
        }

        Collider2D existing = Physics2D.OverlapCircle(pos2D, 0.5f, towerLayer);
        if (existing != null)
        {
            Debug.Log("이미 타워가 있습니다!");
            return;
        }

        GoldManager.Inst.SpendGold(towerCosts[_selectedIndex]);

        Destroy(_previewTower);
        GameObject tower = Instantiate(
            towerPrefabs[_selectedIndex],
            new Vector3(pos2D.x, pos2D.y, 0),
            Quaternion.identity
        );

        tower.layer = LayerMask.NameToLayer("Tower");
        foreach (Transform child in tower.GetComponentsInChildren<Transform>())
            child.gameObject.layer = LayerMask.NameToLayer("Tower");

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