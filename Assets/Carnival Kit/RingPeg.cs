using System.Collections.Generic;
using UnityEngine;

// Put this on a child object of the peg with a trigger collider covering the lower part of the peg.
public class RingPeg : MonoBehaviour
{
    public int points = 1;

    [Tooltip("Max sideways distance (m) between the ring's centre and the peg's centre line")]
    public float maxOffCenter = 0.03f;
    [Tooltip("Max tilt (degrees) from lying flat")]
    public float maxTilt = 30f;
    [Tooltip("Max speed (m/s) for the ring to count as settled")]
    public float maxSpeed = 0.3f;
    [Tooltip("Seconds the ring must stay settled on the peg")]
    public float settleTime = 0.4f;

    class Entry
    {
        public int colliders;
        public float time;
    }

    RingTossGame game;
    readonly Dictionary<Ring, Entry> inside = new Dictionary<Ring, Entry>();
    readonly List<Ring> keys = new List<Ring>();

    void Start()
    {
        game = GetComponentInParent<RingTossGame>();
    }

    void OnTriggerEnter(Collider other)
    {
        
        var ring = other.GetComponentInParent<Ring>();
        Debug.Log("Ring entered peg zone: " + ring.name);
        if (ring == null) return;

        if (!inside.TryGetValue(ring, out var e))
        {
            e = new Entry();
            inside[ring] = e;
        }
        e.colliders++;
    }

    void OnTriggerExit(Collider other)
    {
        var ring = other.GetComponentInParent<Ring>();
        if (ring == null || !inside.TryGetValue(ring, out var e)) return;

        e.colliders--;
        if (e.colliders <= 0) inside.Remove(ring);
    }

    void FixedUpdate()
    {
        if (game == null || inside.Count == 0) return;

        keys.Clear();
        keys.AddRange(inside.Keys);

        foreach (var ring in keys)
        {
            if (ring == null || ring.scored) continue;
            var e = inside[ring];

            Vector3 offset = ring.transform.position - transform.position;
            offset.y = 0f;

            bool ok = !ring.IsHeld
                      && ring.IsFlat(maxTilt)
                      && ring.IsSlow(maxSpeed)
                      && offset.magnitude <= maxOffCenter;

            e.time = ok ? e.time + Time.fixedDeltaTime : 0f;
            if (Time.frameCount % 25 == 0)
                Debug.Log($"{ring.name} ok={ok} held={ring.IsHeld} flat={ring.IsFlat(maxTilt)} slow={ring.IsSlow(maxSpeed)} offset={offset.magnitude:F3}");
            if (e.time >= settleTime)
            {
                ring.scored = true;
                game.AddScore(points);
            }
        }
    }
}