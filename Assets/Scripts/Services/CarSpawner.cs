using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services
{
    public class CarSpawner : MonoBehaviour
    {
        [SerializeField] private CinemachineSmoothPath cinemachinePath;
        public GameObject[] carPrefabs; 
        public Transform platform; 
        public float carSpacing = 2f;

        private Bounds _platformBounds; 
        private HashSet<Vector3> _occupiedPositions = new HashSet<Vector3>(); 
        private float _gridSize;

        void Start()
        {
            _platformBounds = platform.GetComponent<Renderer>().bounds;
            _gridSize = carSpacing;
            SpawnCarsCluster();
        }

        void SpawnCarsCluster()
        {
            Vector3 initialPosition = GetGridAlignedPosition(_platformBounds.center);
            SpawnCarAtPosition(initialPosition);

            Queue<Vector3> positionsToCheck = new Queue<Vector3>();
            positionsToCheck.Enqueue(initialPosition);

            while (positionsToCheck.Count > 0)
            {
                Vector3 currentPosition = positionsToCheck.Dequeue();

                // Проверяем соседние позиции
                Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                foreach (Vector3 direction in directions)
                {
                    Vector3 newPosition = currentPosition + direction * _gridSize;

                    // Если позиция валидна, спавним машину
                    if (IsPositionValid(newPosition))
                    {
                        SpawnCarAtPosition(newPosition);
                        positionsToCheck.Enqueue(newPosition);
                    }
                }
            }
        }

        [Obsolete("Obsolete")]
        void SpawnCarAtPosition(Vector3 position)
        {
            var randomCarPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

            var newCar = Instantiate(randomCarPrefab, position, Quaternion.identity);
            
            newCar.GetComponent<Car.Car>().SetPath(cinemachinePath);

            Vector3 randomDirection = Random.value > 0.5f ? Vector3.forward : Vector3.right;
            newCar.transform.rotation = Quaternion.LookRotation(randomDirection);

            _occupiedPositions.Add(position);
        }

        Vector3 GetGridAlignedPosition(Vector3 position)
        {
            float x = Mathf.Round(position.x / _gridSize) * _gridSize;
            float z = Mathf.Round(position.z / _gridSize) * _gridSize;
            float y = _platformBounds.max.y;
            return new Vector3(x, y, z);
        }

        bool IsPositionValid(Vector3 position)
        {
            if (!_platformBounds.Contains(new Vector3(position.x, _platformBounds.center.y, position.z)))
            {
                return false;
            }

            return !_occupiedPositions.Contains(position);
        }
    }
}
