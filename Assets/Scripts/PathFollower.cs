using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {
    public float speed;
    public float reachDistance = 0.1f;

    private int indexOnPath;

    public Vector3 PositionOnPath { get; private set; }

    private void Start() {
        PlaceOnFirstPoint(transform);
    }

    public void PlaceOnFirstPoint(Transform trs) {
        if (PathManager.Instance.Path.Count <= 0) {
            return;
        }

        Vector3 position = PathManager.Instance.Path[0];
        trs.position = position;
        PositionOnPath = position;
        indexOnPath = 0;
    }

    public void PlaceToClosestPoint(Transform trs) {
        indexOnPath = PathManager.Instance.FindClosestPoint(trs.position);

        List<Vector3> path = PathManager.Instance.Path;
        Vector3 pos = path[indexOnPath];
        trs.position = pos;
        PositionOnPath = pos;
        if (indexOnPath < path.Count - 1) {
            trs.LookAt(path[indexOnPath + 1]);
        }
    }

    public void UpdatePositionOnPath() {
        List<Vector3> path = PathManager.Instance.Path;
        if (indexOnPath >= path.Count - 1) {
            // Loop back to start if at last point.
            PlaceOnFirstPoint(transform);
            return;
        }

        Vector3 nextPoint = path[indexOnPath + 1];
        float distance = Vector3.Distance(PositionOnPath, nextPoint);
        if (distance <= reachDistance) {
            ++indexOnPath;
        }

        PositionOnPath = Vector3.MoveTowards(PositionOnPath, nextPoint, Time.deltaTime * speed);
    }
}