using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SoftBody : MonoBehaviour
{

    [field: SerializeField]
    private float spring = 100f;
    [field: SerializeField]
    private float damper = 0.2f;
    [field: SerializeField]
    private float tolerance = 0.025f;
    [field: SerializeField]
    private float centerSpring = 100f;
    [field: SerializeField]
    private float centerDamper = 0.2f;
    [field: SerializeField]
    private float centerTolerance = 0.025f;
    [field: SerializeField]
    private bool spriteMode = false;
    [field: SerializeField]
    private Sprite sprite;
    
    [field: SerializeField]
    private GameObject node;

    private Mesh mesh;
    private Mesh newMesh;
    private MeshRenderer mRenderer;
    private Vector3[] vertices;
    private int[] triangles;
    private Node[] nodes;
    private Vector3[] vertexArr;
    private List<int>[] neighbors;

    private void Start(){
        // Sprite mode
        if (spriteMode){
            mesh = new Mesh();
            mesh.vertices = Array.ConvertAll(sprite.vertices, v => (Vector3) v);
            mesh.uv = sprite.uv;
            mesh.triangles = Array.ConvertAll(sprite.triangles, t => (int) t);
        }else{
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        // Correct vertices
        List<Vector3> verticesList = new List<Vector3>();
        for (int i = 0; i < mesh.vertices.Length; i++){
            if (!verticesList.Contains(mesh.vertices[i])){
                verticesList.Add(mesh.vertices[i]);
            }
        }
        vertices = verticesList.ToArray();
        // Correct triangles
        List<int> trianglesList = new List<int>();
        for (int i = 0; i < mesh.triangles.Length; i++){
            trianglesList.Add(Array.IndexOf(vertices, mesh.vertices[mesh.triangles[i]]));
        }
        triangles = trianglesList.ToArray();

        // Find neighbors
        List<int>[] neighborsUF = new List<int>[vertices.Length];
        for (int i = 0; i < vertices.Length; i++){
            neighborsUF[i] = new List<int>();
            for (int x = 0; x < triangles.Length; x += 3){
                if (triangles[x] == i){
                    float farthest = Mathf.Max(Vector3.Distance(vertices[triangles[x + 1]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x + 1]]));
                    if (farthest != Vector3.Distance(vertices[triangles[x + 1]], vertices[triangles[x]])){
                        neighborsUF[i].Add(triangles[x + 1]);
                    }
                    if (farthest != Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x]])){
                        neighborsUF[i].Add(triangles[x + 2]);
                    }
                }else if (triangles[x + 1] == i){
                    float farthest = Mathf.Max(Vector3.Distance(vertices[triangles[x + 1]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x + 1]]));
                    if (farthest != Vector3.Distance(vertices[triangles[x + 1]], vertices[triangles[x]])){
                        neighborsUF[i].Add(triangles[x]);
                    }
                    if (farthest != Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x + 1]])){
                        neighborsUF[i].Add(triangles[x + 2]);
                    }
                }else if (triangles[x + 2] == i){
                    float farthest = Mathf.Max(Vector3.Distance(vertices[triangles[x + 1]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x]]), Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x + 1]]));
                    if (farthest != Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x]])){
                        neighborsUF[i].Add(triangles[x]);
                    }
                    if (farthest != Vector3.Distance(vertices[triangles[x + 2]], vertices[triangles[x + 1]])){
                        neighborsUF[i].Add(triangles[x + 1]);
                    }
                }
            }
        }
        neighbors = new List<int>[vertices.Length];
        for (int i = 0; i < vertices.Length; i++){
            Vector3 currentVertex = vertices[i];
            neighbors[i] = neighborsUF[i].Distinct().ToList();
        }

        nodes = new Node[vertices.Length];
        for (int i = 0; i < vertices.Length; i++){
            nodes[i] = new Node(node, neighbors[i], vertices[i], nodes, transform);
        }
        for (int i = 0; i < nodes.Length; i++){
            nodes[i].AddSprings(spring, damper, tolerance, centerSpring, centerDamper, centerTolerance);
        }

        newMesh = Instantiate(mesh);
        newMesh.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh = newMesh;
        mRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate(){
        foreach (Node node in nodes){
            node.UpdateSprings(spring, damper, tolerance, centerSpring, centerDamper, centerTolerance);
            node.go.transform.localRotation = Quaternion.identity;
        }
        vertexArr = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < vertexArr.Length; i++){
            vertexArr[i] = nodes[Array.IndexOf(vertices, mesh.vertices[i])].go.transform.localPosition;
        }
        newMesh.vertices = vertexArr;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = newMesh;
        foreach (Node node in nodes){
            node.go.transform.localRotation = Quaternion.identity;
        }
    }

    public class Node{
        public GameObject prefab;
        public GameObject go;
        public List<int> nodeNeighbors;
        public Rigidbody rb;
        public Node[] nodes;
        public Transform transform;

        public Node(GameObject _prefab, List<int> _nodeNeighbors, Vector3 _pos, Node[] _nodes, Transform _transform){
            nodes = _nodes;
            transform = _transform;
            prefab = _prefab;
            nodeNeighbors = _nodeNeighbors;
            go = Instantiate(prefab);
            go.transform.parent = transform;
            go.transform.localPosition = _pos;
            go.transform.localRotation = Quaternion.identity;
            rb = go.GetComponent<Rigidbody>();
        }
        
        public void AddSprings(float _spring, float _damper, float _tolerance, float _centerSpring, float _centerDamper, float _centerTolerance){
            for (int i = 0; i < nodeNeighbors.Count; i++){
                SpringJoint sj = go.AddComponent<SpringJoint>();
                sj.connectedBody = nodes[nodeNeighbors[i]].rb;
                sj.autoConfigureConnectedAnchor = true;
                sj.damper = _damper;
                sj.spring = _spring;
                sj.tolerance = _tolerance;
            }
            SpringJoint cj = go.AddComponent<SpringJoint>();
            cj.connectedBody = transform.gameObject.GetComponent<Rigidbody>();
            cj.autoConfigureConnectedAnchor = true;
            cj.damper = _centerDamper;
            cj.spring = _centerSpring;
            cj.tolerance = _centerTolerance;
            Physics.IgnoreCollision(go.GetComponent<SphereCollider>(), transform.GetComponent<MeshCollider>());
        }

        public void UpdateSprings(float _spring, float _damper, float _tolerance, float _centerSpring, float _centerDamper, float _centerTolerance){
            for (int i = 0; i < nodeNeighbors.Count; i++){
                SpringJoint sj = go.GetComponents<SpringJoint>()[i];
                sj.damper = _damper;
                sj.spring = _spring;
                sj.tolerance = _tolerance;
            }
            SpringJoint cj = go.GetComponents<SpringJoint>()[nodeNeighbors.Count];
            cj.damper = _centerDamper;
            cj.spring = _centerSpring;
            cj.tolerance = _centerTolerance;
        }
    }
}
