using System.Collections.Generic;
using UnityEngine;

public class PathManager : Singleton<PathManager> {
    // Controls the slide's shape and how the player interacts with it.
    public AnimationCurve slideCurve;
    public Material slideMaterial;

    [Range(1.0f, 8.0f)] public float slideXScale = 2f;
    [Range(1.0f, 8.0f)] public float slideYScale = 1f;

    // Number of vertices on one point of the path.
    [Range(3, 32)] public int meshResolution = 8;

    // Collection of all the positions that a PathFollower goes to.
    public List<Vector3> Path { get; private set; }

    protected override void Awake() {
        base.Awake();

        int childCount = transform.childCount;
        Path = new List<Vector3>(childCount);
        for (int i = 0; i < childCount; ++i) {
            Path.Add(transform.GetChild(i).position);
        }
    }

    private void Start() {
        CreateSlideMeshAndGameObject();
    }

    public Vector2 GetPositionOnSlideCurve(float xPos) {
        return new Vector2(xPos * slideXScale, slideCurve.Evaluate(xPos) * slideYScale);
    }

    public void CreateSlideMeshAndGameObject() {
        int pathCount = Path.Count;
        int vertexCount = meshResolution * pathCount;
        var vertices = new Vector3[vertexCount];

        // Width * height {number of quads} * 2 {number of triangles per quad} * 3 {number of indices per triangle}.
        int indexCount = (meshResolution - 1) * (pathCount - 1) * 2 * 3;
        var triangles = new int[indexCount];

        // todo: slide mesh orientation from path direction, mesh uvs, mesh thickness
        var mesh = new Mesh { name = "slide" };

        // Vertices
        for (int i = 0; i < pathCount; ++i) {
            Vector3 basePosition = Path[i];
            for (int j = 0; j < meshResolution; ++j) {
                // Evaluate slide curve at points between -1 and 1 to get the XY position of current vertex.
                float f = -1.0f + j / (float) (meshResolution - 1) * 2.0f;
                Vector2 offset = GetPositionOnSlideCurve(f);

                vertices[meshResolution * i + j] =
                    new Vector3(basePosition.x + offset.x, basePosition.y + offset.y, basePosition.z);
            }
        }

        // Triangles
        int currentIndex = 0; // To access {triangles} array in order.
        for (int i = 0; i < vertexCount; ++i) {
            if (i >= vertexCount - meshResolution) {
                break;
            }

            if (i % meshResolution == meshResolution - 1) {
                continue;
            }

            int BL = i; //   TL *--* TR
            int BR = i + 1; //      |\ |
            int TL = i + meshResolution; //      | \|   
            int TR = i + meshResolution + 1; //   BL *--* BR

            // First triangle
            triangles[currentIndex++] = BL;
            triangles[currentIndex++] = TL;
            triangles[currentIndex++] = BR;
            // Second triangle
            triangles[currentIndex++] = TL;
            triangles[currentIndex++] = TR;
            triangles[currentIndex++] = BR;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        var go = new GameObject("Slide", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)) {
            isStatic = true
        };
        go.GetComponent<MeshRenderer>().sharedMaterial = slideMaterial;
        go.GetComponent<MeshFilter>().mesh = mesh;

        var meshCollider = go.GetComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        meshCollider.sharedMesh = mesh;
    }

    public int FindClosestPoint(Vector3 position) {
        int closestPoint = 0;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < Path.Count; ++i) {
            float distance = Vector3.Distance(position, Path[i]);
            if (distance > shortestDistance) {
                continue;
            }

            closestPoint = i;
            shortestDistance = distance;
        }

        return closestPoint;
    }

    // Path visualization in editor.
    private void OnDrawGizmos() {
        int childCount = transform.childCount;
        if (childCount < 1)
            return;

        Gizmos.DrawWireSphere(transform.GetChild(0).position, 0.5f);
        for (int i = 1; i < childCount; ++i) {
            Transform child = transform.GetChild(i);
            Transform previousChild = transform.GetChild(i - 1);

            Vector3 childPos = child.position;
            Gizmos.DrawWireSphere(childPos, 0.5f);
            Gizmos.DrawLine(previousChild.position, childPos);
        }
    }
}