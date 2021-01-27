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

    public struct LeafData
    {
        public Transform Transform;
        public LeafController Controller;
    }

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

    public void SpawnLeafsRandomly(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(0, 2f * Mathf.PI);
            float randomRadius = Random.Range(0, 4.5f);
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));

            Vector2 position = MathUtils.PolarToRect(randomAngle, randomRadius);
            
            GameObject newLeaf = Instantiate(leafPrefab, position, Quaternion.identity);
            LeafController controller = newLeaf.GetComponent<LeafController>();
            controller.HeightInAir = 0;
            // Set rotation through Leaf so that it can handle the rotation properly (only rotates sprite child)
            newLeaf.GetComponent<LeafInterp>().SetRotation(randomRotation);

            LeafData leafData = new LeafData()
            {
                Transform = newLeaf.transform,
                Controller = controller
            };
            
            _leafDatas[nextIdToUse] = leafData;
            _lastPositions[nextIdToUse] = position;
            _lastHeightsInAir[nextIdToUse] = controller.HeightInAir;
            
            nextIdToUse++;
        }
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
                    rotation = leafTransform.rotation,
                    heightInAir = leafController.HeightInAir
                };
                leafStates[key] = leafState;
                
                // Update the last positions for next time
                _lastPositions[key] = leafTransform.position;
                _lastHeightsInAir[key] = leafController.HeightInAir;
            }
        }

        return leafStates;
    }

    public GameObject SpawnLeaf(Vector2 position, Quaternion rotation)
    {
        GameObject newLeaf = Instantiate(leafPrefab, position, rotation);
        return newLeaf;
    }

    public List<short> GetSectorLeafCounts(short segments, float offset)
    {
        short[] counts = new short[segments];

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
