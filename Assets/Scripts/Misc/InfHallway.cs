using UnityEngine;
using System.Collections.Generic;

public class InfiniteHallwayGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject hallwaySegmentPrefab;
    
    [Header("Settings")]
    [SerializeField] private int initialSegments = 5;
    [SerializeField] private int maxSegments = 10;
    [SerializeField] private float segmentLength = 10f;
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float despawnDistance = 40f;
    
    private List<GameObject> hallwaySegments = new List<GameObject>();
    private Vector3 nextSpawnPosition;
    
    private void Start()
    {
        if (player == null)
            player = Camera.main.transform;
            
        nextSpawnPosition = transform.position;
        
        // Create initial segments
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnNewSegment();
        }
    }
    
    private void Update()
    {
        // Check if we need to spawn new segments
        float distanceToLastSegment = Vector3.Distance(player.position, nextSpawnPosition - new Vector3(0, 0, segmentLength));
        
        if (distanceToLastSegment < spawnDistance && hallwaySegments.Count < maxSegments)
        {
            SpawnNewSegment();
        }
        
        // Check if we need to remove far segments
        List<GameObject> segmentsToRemove = new List<GameObject>();
        
        foreach (GameObject segment in hallwaySegments)
        {
            float distanceToSegment = Vector3.Distance(player.position, segment.transform.position);
            
            if (distanceToSegment > despawnDistance)
            {
                segmentsToRemove.Add(segment);
            }
        }
        
        // Remove segments that are too far away
        foreach (GameObject segment in segmentsToRemove)
        {
            hallwaySegments.Remove(segment);
            Destroy(segment);
        }
    }
    
    private void SpawnNewSegment()
    {
        GameObject newSegment = Instantiate(hallwaySegmentPrefab, nextSpawnPosition, Quaternion.identity, transform);
        hallwaySegments.Add(newSegment);
        
        // Update next spawn position
        nextSpawnPosition += new Vector3(0, 0, segmentLength);
    }
}