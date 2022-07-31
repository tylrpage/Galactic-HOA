using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;
using Random = UnityEngine.Random;

public class LeafSpawner : MonoBehaviour
{
    // TODO: use object pool
#pragma warning disable 0649
    [SerializeField] private GameObject leafPrefab;
#pragma warning restore 0649

    public class LeafData
    {
        public Transform Transform;
        public LeafController Controller;
        public bool IsNew;
    }

    public float MinSpawningHeight;
    public float MaxSpawningHeight;
    public float SpawningDuration;
    public short RoughlyNumberOfLeafsToSpawn;

    private Dictionary<int, LeafData> _leafDatas;
    private Dictionary<int, Vector2> _lastPositions;
    private Dictionary<int, float> _lastHeightsInAir;
    private int nextIdToUse = 0;

    private void Awake()
    {
        _lastPositions = new Dictionary<int, Vector2>();
        _lastHeightsInAir = new Dictionary<int, float>();
        _leafDatas = new Dictionary<int, LeafData>();
    }

    public void ClearAllLeafs()
    {
        nextIdToUse = 0;
        foreach (var leafData in _leafDatas.Values)
        {
            Destroy(leafData.Transform.gameObject);
        }
        _leafDatas.Clear();
        _lastPositions.Clear();
        _lastHeightsInAir.Clear();
    }

    public void SpawnLeafsOverTime()
    {
        StartCoroutine(SpawnLeafsOverTimeRoutine(SpawningDuration, RoughlyNumberOfLeafsToSpawn));
    }

    private IEnumerator SpawnLeafsOverTimeRoutine(float duration, short numberOfLeafs)
    {
        int leafsSpawned = 0;
        for (int i = 0; i < duration / 0.5f; i++)
        {
            int leafsToSpawn = Random.Range(1, (numberOfLeafs - leafsSpawned) / 2);
            leafsSpawned += leafsToSpawn;
            for (int j = 0; j < leafsToSpawn; j++)
            {
                SpawnRandomServerLeaf();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public GameObject SpawnLeaf(int id, Vector2 position, float height, Quaternion rotation, bool enablePhysics)
    {
        GameObject newLeaf = Instantiate(leafPrefab, position, Quaternion.identity);
        LeafController controller = newLeaf.GetComponent<LeafController>();
        controller.HeightInAir = height;
        
        // Set rotation through Leaf so that it can handle the rotation properly (only rotates sprite child)
        newLeaf.GetComponent<LeafInterp>().SetRotation(rotation);

        newLeaf.GetComponent<Rigidbody2D>().simulated = enablePhysics;
        
        LeafData leafData = new LeafData()
        {
            Transform = newLeaf.transform,
            Controller = controller,
            IsNew = true
        };
            
        _leafDatas[id] = leafData;
        _lastPositions[id] = position;
        _lastHeightsInAir[id] = controller.HeightInAir;

        return newLeaf;
    }

    private void SpawnRandomServerLeaf()
    {
        float randomAngle = Random.Range(0, 2f * Mathf.PI);
        float randomRadius = Random.Range(0, 4.5f);
        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));

        Vector2 position = MathUtils.PolarToRect(randomAngle, randomRadius);
        
        float randomHeight = Random.Range(MinSpawningHeight, MaxSpawningHeight);
        GameObject newLeaf = SpawnLeaf(nextIdToUse, position, randomHeight, randomRotation, true);
        LeafController controller = newLeaf.GetComponent<LeafController>();
        controller.Simulate = true; // allow server to simulate the falling of leaves

        nextIdToUse++;
    }

    public Dictionary<int, LeafState> GenerateLeafStates(bool onlySendDirty)
    {
        var leafStates = new Dictionary<int, LeafState>();
        foreach (var keyValue in _leafDatas)
        {
            Transform leafTransform = keyValue.Value.Transform;
            LeafController leafController = keyValue.Value.Controller;

            int key = keyValue.Key;
            if (_lastPositions[key] != (Vector2) leafTransform.position || _lastHeightsInAir[key] != leafController.HeightInAir || !onlySendDirty)
            {
                LeafState leafState = new LeafState()
                {
                    position = leafTransform.position,
                    rotation = leafController.SpriteTransform.rotation,
                    heightInAir = leafController.HeightInAir,
                    IsNew = keyValue.Value.IsNew
                };
                leafStates[key] = leafState;
                
                // Update the last positions for next time
                _lastPositions[key] = leafTransform.position;
                _lastHeightsInAir[key] = leafController.HeightInAir;
                // Leaf is no longer new
                keyValue.Value.IsNew = false;
            }
        }

        return leafStates;
    }

    public List<ushort> GetSectorLeafCounts(short segments, float offset)
    {
        ushort[] counts = new ushort[segments];

        if (segments > 0)
        {
            foreach (var leafData in _leafDatas.Values)
            {
                Transform leafTransform = leafData.Transform;
                
                Vector2 polar = MathUtils.RectToPolar(leafTransform.position);
                float degree = MathUtils.RadiansToDegree(polar.x);
            
                float leafAngle = degree - offset;
                if (leafAngle < 0)
                    leafAngle += 360;
            
                int segment = Mathf.FloorToInt(leafAngle / (360f / segments));
                counts[segment]++;
            }
        }
 
        return counts.ToList();
    }
}
