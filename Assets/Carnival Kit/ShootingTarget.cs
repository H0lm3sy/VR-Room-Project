using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreZone
{
    public string label = "Bullseye";
    [Tooltip("Empty child object placed at the middle of this zone")]
    public Transform center;
    [Tooltip("Radius in metres. A shot within this distance of the centre scores this zone.")]
    public float radius = 0.05f;
    public int points = 10;
    public Color gizmoColor = Color.red;
}

public class ShootingTarget : MonoBehaviour
{
    [Tooltip("Zones, in any order. If a shot lands in several, the highest points win.")]
    public ScoreZone[] zones;

    [Header("Hitting the board outside every zone")]
    public int boardPoints = 1;
    public string boardLabel = "Board";

    [Header("Optional")]
    [Tooltip("Small object (e.g. a flat dark quad) spawned where each shot lands")]
    public GameObject holePrefab;
    public AudioClip hitSound;

    ShootingBooth booth;
    readonly List<GameObject> holes = new List<GameObject>();

    void Start()
    {
        booth = GetComponentInParent<ShootingBooth>();
    }

    public void Hit(Vector3 point, Vector3 normal)
    {
        int best = boardPoints;
        string bestLabel = boardLabel;

        foreach (var z in zones)
        {
            if (z.center == null) continue;
            if (Vector3.Distance(point, z.center.position) <= z.radius && z.points > best)
            {
                best = z.points;
                bestLabel = z.label;
            }
        }

        if (holePrefab != null)
            holes.Add(Instantiate(holePrefab, point + normal * 0.002f, Quaternion.LookRotation(-normal)));

        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, point);
        if (booth != null) booth.OnTargetHit(best, bestLabel);
    }

    public void ResetState()
    {
        foreach (var h in holes)
            if (h != null) Destroy(h);
        holes.Clear();
    }

    // Scoring circles are always drawn in the Scene view (turn Gizmos on)
    void OnDrawGizmos()
    {
        if (zones == null) return;
        foreach (var z in zones)
        {
            if (z.center == null) continue;
            Gizmos.color = z.gizmoColor;
            Gizmos.DrawWireSphere(z.center.position, z.radius);
        }
    }
}