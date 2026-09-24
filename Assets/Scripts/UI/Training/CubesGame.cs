using System;
using System.Collections.Generic;
using System.Linq;
using DialogueEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Training
{
    public class CubesGame : MonoBehaviour
    {
        [SerializeField] private NPCConversation endTraining;
    
        [SerializeField] private GameObject table;
    
        private bool _cubesGameStarted;
            
        private bool _trainingEnded;

        private int _cubesCount;
    
        [SerializeField] private List<GameObject> cubes = new List<GameObject>();
    
        public void CubesGameBool(bool cubesBool) => _cubesGameStarted = cubesBool;

        private void Start()
        {
            _cubesCount = GameObject.FindGameObjectsWithTag("Cube").Length;
        }

        private void Update()
        {
            if (cubes.Count < _cubesCount || _trainingEnded || !_cubesGameStarted) return;
            
            _trainingEnded = true;
            table.SetActive(true);
            ConversationManager.Instance.StartConversation(endTraining);
        }
    
    
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Cube") || !_cubesGameStarted) return;
            
            if (cubes.Any(cube => cube.name == other.gameObject.name))
            {
                return;
            }
            
            cubes.Add(other.gameObject);
        }
    
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Cube") || !_cubesGameStarted) return;

            foreach (var cube in cubes.Where(cube => cube.name == other.gameObject.name))
            {
                cubes.Remove(cube);
                return;
            }
        }
    }
}
