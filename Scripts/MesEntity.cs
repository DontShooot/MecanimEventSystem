using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MecanimEventSystem
{
    /// <summary>
    /// This class contains all that you need. Just assign it to entity and subscribe to OnStateChanged
    /// Animator should be assigned to the same gameobject or it's child.
    /// If you instantiating gameobject with Animator later, you can just set link to Animator's controller in the OverrideController variable
    /// </summary>
    public class MesEntity : MonoBehaviour
    {
        [SerializeField] private List<MesStateData> _states;
        [SerializeField] private RuntimeAnimatorController _overrideController;


        //(Old state, New state)
        public event Action<string, string> OnStateChanged = (s1, s2) => { };

        private Animator _animator;

        public RuntimeAnimatorController OverrideController
        {
            get { return _overrideController; }
        }

        public string CurrentStateName
        {
            get
            {
                if (!_currentStateHash.HasValue) return string.Empty;
                
                MesStateData curStateData = _states.FirstOrDefault(x => x.NameHash == _currentStateHash);
                if (curStateData == null) return string.Empty;

                return curStateData.StateName;
            }
        }

        public Animator Animator
        {
            get { return _animator; }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (Animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (Animator == null)
                Debug.LogError("Failed to find animator component");

            var defaultState = _states.First(s => s.IsDefaultState);
            _currentStateHash = defaultState.NameHash;
        }

        private int? _currentStateHash;
        private void Update()
        {
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (!_currentStateHash.HasValue || stateInfo.nameHash != _currentStateHash)
            {
                NotifyStateChanged(_currentStateHash, stateInfo.nameHash);
                _currentStateHash = stateInfo.nameHash;
            }
        }

        private void NotifyStateChanged(int? oldState, int newState)
        {
            if (_states == null || _states.Count == 0)
            {
                Debug.Log("No states info");
                return;
            }

            MesStateData newStateData = _states.FirstOrDefault(x => x.NameHash == newState);
            if (newStateData == null)
            {
                Debug.LogError("Failed to find new state data");
                return;
            }

            string newStateName = newStateData.StateName;

            MesStateData oldStateData = _states.FirstOrDefault(x => x.NameHash == oldState);

            string oldStateName = (oldStateData == null) ? String.Empty : oldStateData.StateName;

            OnStateChanged(oldStateName, newStateName);
        }

        public void StoreStates(List<MesStateData> states)
        {
            _states = states;
        }
    }
}