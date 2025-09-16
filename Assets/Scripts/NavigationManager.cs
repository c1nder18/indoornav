using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
//using Unity.VisualScripting;
//using UnityEngine.AI;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    // Singleton instance
    // this is global access point. make sure only one exist
    public static NavigationManager Instance { get; private set; }

    [Header("Setup")]
    //[Tooltip("Dropdown containing all possible destinations and the currently selected one.")]
    // public TMP_Dropdown destinations;
    public GameObject destinationsMenu;     // Menu containing all destinations, is categorized (这里放分类后的目的地菜单)
    public GameObject destinationBtn;       // Button which opens the menu (打开菜单的按钮)
    public TextMeshProUGUI destinationText; // Text showing the name of the destination (显示当前目的地名字)
    public TextMeshProUGUI directionsText;  // Text showing directions to next location in path (文本提示下一步指引)
    public GameObject[] locations;          // All existing locations (场景中所有节点)
    public Edge[] edges;                    // All existing edges (节点之间的边/连线，里面有方向提示文案)
    public GameObject exit;                 // Exit location (出口节点)

    public GameObject finishOverlay;        // Prefab that instantiates when user gets to finish (到达终点时弹出的UI)
    public GameObject canvas;               // root canvas for UI (主Canvas对象，方便实例化子UI)


    [Header("Debug")]
    public Toggle arrowsToggle;             // only use arrow mode now. when off, hide path arrows
                                            //public Toggle lineToggle;             // line mode already removed (废弃掉的线导航Toggle，保留注释方便以后回看)

    [Header("Immersal Path Navigation")]
    public GameObject testCam;  // Used for testing (编辑器里用这个当主相机，真机用Camera.main)
    public Streamline lineNear; // deprecated, keep reference so prefab不报错 (近线，已废弃)
    public Streamline lineFar;  // deprecated (远线，已废弃)

    private GameObject camera; // user camera (当前使用中的相机GameObject)
    private List<GameObject> _restrooms = new List<GameObject>(); // collect WC locations for quick find
    private GameObject _location;   // current location (用户当前所在节点)
    private GameObject _destination;// current destination (用户目标节点)
    //private string lastLocation;   // old code, we keep but not use

    // path is a list of edges with direction flag. we use LinkedList for fast remove from head
    private LinkedList<(Edge edge, bool isForward)> _path; // Tuple to store Edge and direction
    private Dictionary<string, List<Edge>> _adjacencyList; // adjacency for BFS, key is location name

    private void Awake()
    {
        // make sure only one instance alive. if second appear, destroy it.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // keep manager across scene (如果换场景也不销毁)
    }

    private void Start()
    {
        BuildAdjacencyList();  // 根据edges建图，后面BFS用
        Hide(edges);           // 初始把所有边隐藏，等选择目的地再显示

        // Gather all restroom locations into a list
        // loop all locations, check type flag include WC, then push to list
        foreach (GameObject location in locations)
        {
            if ((location.GetComponent<Location>().type & Location.Types.WC) != 0)
            {
                _restrooms.Add(location);
            }
        }

        // get runtime camera. in editor we prefer testCam for convenient simulate
        camera = Camera.main?.gameObject;
#if UNITY_EDITOR
        camera = testCam;
#endif
    }

    //private void Update()
    //{
    //    // old line mode update (已经不用，但保留给以后参考)
    //    if (_path != null)
    //    {
    //        // Update streamlines
    //        GameObject nextLocation = GetNextLocationInPath();
    //        if (nextLocation != null && lineToggle.isOn)
    //        {
    //            lineNear.ShowFromAToB(camera, nextLocation.GetComponent<Location>().navigationTarget.gameObject);
    //            lineFar.ShowFromAToB(GetNextLocationInPath().gameObject, _destination.gameObject);
    //        }
    //    }
    //}

    // Dictionary of "What edges am I connected to" for each location
    // build graph: for each edge, add to both nodeA and nodeB list (无向图)
    private void BuildAdjacencyList()
    {
        _adjacencyList = new Dictionary<string, List<Edge>>();
        foreach (Edge edge in edges)
        {
            string nodeA = edge.a.name;
            string nodeB = edge.b.name;

            // Add edge to adjacency list for location A
            if (!_adjacencyList.ContainsKey(nodeA))
            {
                _adjacencyList[nodeA] = new List<Edge>();
            }
            _adjacencyList[nodeA].Add(edge);

            // Add edge to adjacency list for location B
            if (!_adjacencyList.ContainsKey(nodeB))
            {
                _adjacencyList[nodeB] = new List<Edge>();
            }
            _adjacencyList[nodeB].Add(edge);
        }

    }

    // Check what toggle changed and update its navigation method accordingly
    // now only arrowsToggle is useful. when off -> hide path; when on -> show path again
    public void ToggleChanged(Toggle toggle)
    {
        if (toggle == arrowsToggle && !toggle.isOn) Hide(_path);
        else if (toggle == arrowsToggle && toggle.isOn) ShowPath();
        //else if (toggle == lineToggle && !toggle.isOn) HideLines();
    }

    // Hide streamlines
    // keep old method as comment, so no missing reference if someone search
    //private void HideLines()
    //{
    //    lineNear.HideNavmeshPath();
    //    lineFar.HideNavmeshPath();
    //}

    // When user has come to a new location
    // usually called by some tracker (比如AR定位或者触发器)
    public void LocationChanged(GameObject newLocation)
    {
        //lastLocation = location;
        _location = newLocation; // 更新当前位置
        UpdatePath();            // 根据当前位置把已经走过的边pop掉
        ShowDirections();        // 更新文字提示
    }

    // When user selects a new destination
    // change destination by name (菜单点击触发)
    public void DestinationChanged(string newDestination)
    {
        // string destName = destinations.options[destinations.value].text;

        if (_destination != null) _destination.GetComponent<Location>().isDestination = false; // disable last (取消旧的终点标记)
        _destination = locations.FirstOrDefault(x => x.name == newDestination);       // Find the location (根据名字找GameObject)
        _destination.GetComponent<Location>().isDestination = true;                             // set the new (打上新终点标记，可能会改变美术样式)

        destinationText.text = newDestination;  // Update destination text (UI显示新目的地名字)
        destinationBtn.SetActive(true);         // Show destination menu button (显示“选择目的地”按钮)
        destinationsMenu.SetActive(false);      // Hide destination menu (关闭弹出的菜单)


        Hide(_path);     // 把当前路径上的箭头先隐藏
        FindPath();      // 用BFS重新算最短路径
        //lineToggle.isOn = true;
        arrowsToggle.isOn = true; // 强制打开箭头模式，确保能看见
        ShowPath();      // 把新路径上的箭头显示出来
        ShowDirections();// 更新文本方向
    }

    // Show text directions
    // decide what to write on directionsText, simple rule: show first edge's direction text
    private void ShowDirections()
    {
        if (_path == null)
        {
            directionsText.text = ""; // 没有路径就不提示
            return;
        }
        else if (_path.Count == 0)
        {
            directionsText.text = ""; // 走完了
            Instantiate(finishOverlay, gameObject.transform); // 弹完成UI
            return;
        }

        // 根据方向选择文案：从A到B用fromAToB，反之用fromBToA
        directionsText.text = First().isForward ? First().edge.fromAToB : First().edge.fromBToA;
    }

    // Multipurpose function to hide edges
    // pass Edge[] or LinkedList<(Edge,bool)>, both supported
    public void Hide(IEnumerable what)
    {
        if (what == null) return;

        foreach (var edge in what)
        {
            if (edge is Edge e) e.Hide();                   // Here edge is just Edge (直接隐藏)
            else if (edge is (Edge _e, bool _)) _e.Hide();  // Here it is in a tuple (路径里的元素，取出Edge隐藏)
            else return; // not supported type, just exit
        }
    }

    // helper: get current node at head of path (这个是“正在离开的节点”)
    public GameObject GetCurrentLocationInPath()
    {
        if (_path == null || _path.Count == 0) return null;
        return First().isForward ? First().edge.a : First().edge.b;
    }

    // helper: next node we go to (这个是“下一个要去的节点”)
    public GameObject GetNextLocationInPath()
    {
        if (_path == null || _path.Count == 0) return null;
        return First().isForward ? First().edge.b : First().edge.a;
    }

    // First edge in path, returns a tuple
    // use property-like method to make code short
    private (Edge edge, bool isForward) First()
    {
        return _path.First.Value;
    }

    // do we have a valid destination? (有就显示箭头和文本，没有就不显示)
    public bool HasDestination()
    {
        return _destination != null;
    }

    // change destination by object (外部直接传GameObject也可以)
    public void ChangeDestination(GameObject location)
    {
        DestinationChanged(location.name);
    }

    // Could be remade so that it just searches using type and BFS
    // find nearest WC: try each WC as temporary destination, take shortest path
    public void FindNearestRestroom()
    {
        // if already going to WC, then nothing to do
        if (_restrooms.Contains(locations.FirstOrDefault(loc => loc == _destination))) return;

        Hide(this._path); // 先把当前路径隐藏
        List<LinkedList<(Edge edge, bool isForward)>> paths = new List<LinkedList<(Edge, bool)>>(); // collect candidate paths

        foreach (GameObject wc in _restrooms)
        {
            _destination = wc; // 临时把终点改成WC
            FindPath();        // 算一下路径
            paths.Add(this._path); // 存起来
        }

        // pick the shortest path (用Count比较，越少越近)
        var path = paths.Aggregate((prev, next) => prev.Count > next.Count ? next : prev);
        var val = path.Last.Value; // get last edge, then decide real WC node (有方向，所以要判断去a还是b)
        ChangeDestination(val.isForward ? val.edge.b : val.edge.a); // 真正把目的地切换过去
    }

    // quick go to exit (跳去出口)
    public void FindExit()
    {
        if (exit == _destination) return; // already target exit

        Hide(_path);
        ChangeDestination(exit);
    }

    // When user gets to new location, it updates the path accordingly
    // remove edges we already passed, until head node match our _location
    private void UpdatePath()
    {
        if (_path == null) return;

        // Delete locations user has already been through
        while (_path.Count != 0 && GetCurrentLocationInPath() != _location)
        {
            First().edge.Hide(); // 把已经走过的边隐藏
            _path.RemoveFirst(); // 从路径头部弹出
        }

        // User is in a wrong place, find them a new path
        // 如果一路弹完了还没匹配，说明用户偏航了，需要从当前位置重算
        if (_path.Count == 0)
        {
            FindPath();
            ShowPath();
        }
    }

    // show all arrows on path according to direction
    public void ShowPath()
    {
        if (this._path == null || this._path.Count == 0 || !arrowsToggle.isOn) return;

        foreach (var (edge, isForward) in this._path)
        {
            if (isForward)
            {
                //path += " -> " + edge.b.name;
                edge.ShowForward();  // 显示A->B方向箭头
            }
            else
            {
                //path += " -> " + edge.a.name;
                edge.ShowBackward(); // 显示B->A方向箭头
            }
        }
    }

    // BFS shortest path on unweighted graph
    // store path as list of (edge, direction), not list of nodes,方便直接画箭头
    private void FindPath()
    {
        if (_location == _destination) return; // already arrive

        _path = new LinkedList<(Edge, bool)>();
        Queue<List<(Edge edge, bool isForward)>> queue = new Queue<List<(Edge, bool)>>(); // queue of partial path
        HashSet<GameObject> visited = new HashSet<GameObject>(); // visited nodes to avoid loop

        // Initialize the BFS queue with paths starting from the current location
        queue.Enqueue(new List<(Edge, bool)>());
        visited.Add(_location);

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            GameObject currentNode = _location;

            // Get the last node of the current path
            if (currentPath.Count > 0)
            {
                var lastEdge = currentPath[^1]; // C# ^1 means last index
                currentNode = lastEdge.isForward ? lastEdge.edge.b : lastEdge.edge.a;
            }

            // Check if we've reached the destination
            if (currentNode == _destination)
            {
                _path = new LinkedList<(Edge, bool)>(currentPath); // 把list转成LinkedList
                return; // Exit once the shortest path is found
            }

            // Explore neighbors of the current node
            if (_adjacencyList.ContainsKey(currentNode.name))
            {
                foreach (Edge edge in _adjacencyList[currentNode.name])
                {
                    GameObject neighbor;
                    bool isForward;

                    if (edge.a == currentNode)
                    {
                        neighbor = edge.b;
                        isForward = true;  // 走A->B
                    }
                    else
                    {
                        neighbor = edge.a;
                        isForward = false; // 走B->A
                    }

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        // Create a new path extending the current path
                        var newPath = new List<(Edge, bool)>(currentPath)
                        {
                            (edge, isForward)
                        };
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        // If no path is found, clear the path
        // 没路可走，设为空，这样UI那边会隐藏提示
        _path = null;
    }
}
