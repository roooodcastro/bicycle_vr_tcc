using UnityEngine;

public class PlayerProgress : MonoBehaviour {
    public PathEditor bicycleCircuit;
    public PlayerBicycleController player;
    public float targetTolerance;

    private float totalDistance;
    private float progress;
    private float currentProgress;
    private int targetIndex;
    private bool finished;
    private Transform target;
    private Transform lastTarget;

    void Awake() {
        finished = false;
        progress = 0;
        currentProgress = 0;
        targetIndex = 0;
        totalDistance = bicycleCircuit.CalculateLength();
        target = bicycleCircuit.pathObjects[0];
        lastTarget = target;
    }

    void Update() {
        if (!finished) {
            if (ReachedTarget()) {
                progress += CalculateLastTargetDistance();
                currentProgress = 0;
                if (!SelectNextTarget()) {
                    finished = true;
                    progress = totalDistance;
                    return;
                }
            }

            float sectorLength = Vector3.Distance(lastTarget.position, target.position);
            currentProgress = sectorLength - parallelDistanceFromTarget();
        }
    }

    public bool isFinished() {
        return finished;
    }

    public float RemainingDistance() {
        return totalDistance - (progress + currentProgress);
    }

    private bool ReachedTarget() {
        Vector3 playerPos = player.gameObject.transform.position;
        float distance = Vector3.Distance(playerPos, target.position);
        float parallelDistance = parallelDistanceFromTarget();
        bool closeEnough = distance <= targetTolerance;
        bool reachedParallel = parallelDistance <= 0;
        return closeEnough && reachedParallel;
    }

    private float parallelDistanceFromTarget() {
        Vector3 playerPos = player.gameObject.transform.position;
        Vector3 targetDiff = target.position - lastTarget.position;
        Vector3 nearest = NearestPointOnFiniteLine(lastTarget.position - targetDiff, target.position + targetDiff, playerPos);
        float sectorLength = Vector3.Distance(lastTarget.position, target.position);
        float distToTarget = Vector3.Distance(nearest, target.position);
        float distFromLastTarget = Vector3.Distance(lastTarget.position, nearest);
        distFromLastTarget = distToTarget > sectorLength ? -distFromLastTarget : distFromLastTarget;
        return sectorLength - distFromLastTarget;
    }

    private bool SelectNextTarget() {
        targetIndex++;
        if (targetIndex >= bicycleCircuit.pathObjects.Count)
            return false;
        lastTarget = target;
        target = bicycleCircuit.pathObjects[targetIndex];
        return true;
    }

    private Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt) {
        var line = end - start;
        var len = line.magnitude;
        line.Normalize();

        var v = pnt - start;
        var d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return start + line * d;
    }

    private float CalculateLastTargetDistance() {
        return Vector3.Distance(target.position, lastTarget.position);
    }
}