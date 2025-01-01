using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask PlacmentCheckMask;
    [SerializeField] private LayerMask PlacmentCollideMask;
    [SerializeField] private Camera PlayerCamera;

    private GameObject CurrentPlacingTower;
    private bool isPlacingTower = false;

    private void Update()
    {
        if (CurrentPlacingTower != null && isPlacingTower)
        {
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit HitInfo;

            if (Physics.Raycast(camray, out HitInfo, 100f, PlacmentCollideMask))
            {
                Vector3 adjustedPosition = HitInfo.point;
                Renderer towerRenderer = CurrentPlacingTower.GetComponentInChildren<Renderer>();

                if (towerRenderer != null)
                {
                    adjustedPosition.y += towerRenderer.bounds.extents.y;
                }

                CurrentPlacingTower.transform.position = adjustedPosition;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Destroy(CurrentPlacingTower);
                CurrentPlacingTower = null;
                return;
            }

            if (Input.GetMouseButtonDown(0) && HitInfo.collider != null)
            {
                if (!HitInfo.collider.gameObject.CompareTag("Can'tPlace"))
                {
                    BoxCollider towerCollider = CurrentPlacingTower.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;
                    Vector3 boxCenter = CurrentPlacingTower.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;

                    if (!Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, PlacmentCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        GameLoopMaster.TowersInGame.Add(CurrentPlacingTower.GetComponent<TowerBehavior>());
                        towerCollider.isTrigger = false;
                        isPlacingTower = false;
                        CurrentPlacingTower = null;
                    }
                }
            }
        }
    }

    public void SetTowerToPlace(GameObject tower)
    {
        if (isPlacingTower)
            return;

        CurrentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
        isPlacingTower = true;
    }
}