
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Streamline : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private float pathWidthMax = 0.3f;
    [SerializeField] private float pathWidthMin = 0.06f;
    [SerializeField] private float heightOffset = 0.5f;

    private GameObject pathObject;
    private Immersal.Samples.Navigation.NavigationPath navigationPath;
    private Transform playerTransform;
    
    void Start()
    {/*
        playerTransform = Camera.main?.transform;
#if UNITY_EDITOR
        playerTransform = testCam.transform;
#endif
*/
        if (pathPrefab)
        {
            pathObject = Instantiate(pathPrefab);
            pathObject.SetActive(false);
            navigationPath = pathObject.GetComponent<Immersal.Samples.Navigation.NavigationPath>();
        }
    }
    
/*
    public void ShowNavmeshPathToTarget(Immersal.Samples.Navigation.IsNavigationTarget target)
    {
        if (target == null || playerTransform == null || !NavigationManager.Instance.lineToggle.isOn) return;

        List<Vector3> pathPoints = FindNavmeshPath(playerTransform.position, target.position);
        if (pathPoints.Count < 2) return;

        navigationPath.GeneratePath(pathPoints, Vector3.up);
        navigationPath.pathWidth = pathWidth;
        pathObject.SetActive(true);
    }
*/

    public void ShowFromAToB(GameObject from, GameObject to)
    {
        if (from == null || to == null) return;
        
        List<Vector3> pathPoints = FindNavmeshPath(from.transform.position, to.transform.position);
        if (pathPoints.Count < 2) return;

        navigationPath.GeneratePath(pathPoints, Vector3.up);
        navigationPath.pathWidthMax = pathWidthMax;
        navigationPath.pathWidthMin = pathWidthMin;
        pathObject.SetActive(true);
    }
    
    public void HideNavmeshPath()
    {
        if (pathObject) pathObject.SetActive(false);
    }

    private List<Vector3> FindNavmeshPath(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        List<Vector3> points = new List<Vector3>();

        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            foreach (var point in path.corners)
            {
                points.Add(point + new Vector3(0f, heightOffset, 0f));
            }
        }
        return points;
    }

}
