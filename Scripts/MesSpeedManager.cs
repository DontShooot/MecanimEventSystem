using System.Collections.Generic;
using UnityEngine;

namespace MecanimEventSystem
{
    [RequireComponent(typeof(MesEntity))]
    public class MesSpeedManager : MonoBehaviour
    {
        private class SpeedData
        {
            public float DefaultSpeed { get; set; }
            public float OverrideSpeed { get; set; }
            public bool IsActive { get; set; }
        }

        private MesEntity _mesEntity;
        private Dictionary<string, SpeedData> _overrideSpeed;

        private void Start()
        {
            _mesEntity = GetComponent<MesEntity>();
        }

        public void OverrideSpeed(string stateName, float speed)
        {
            if (_overrideSpeed == null)
            {
                _overrideSpeed = new Dictionary<string, SpeedData>();
            }

            _overrideSpeed[stateName] = new SpeedData() {OverrideSpeed = speed};
        }

        private bool _isDirty;
        private void Update()
        {
            if (_overrideSpeed == null) return;

            if (!_isDirty && _overrideSpeed.ContainsKey(_mesEntity.CurrentStateName))
            {
                var speedData = _overrideSpeed[_mesEntity.CurrentStateName];
                if (!speedData.IsActive)
                {
                    speedData.IsActive = true;
                    speedData.DefaultSpeed = _mesEntity.Animator.speed;
                    _mesEntity.Animator.speed = speedData.OverrideSpeed;
                    _isDirty = true;
                }
            }
            else if (_isDirty)
            {
                foreach (var speedData in _overrideSpeed)
                {
                    if (speedData.Value.IsActive && speedData.Key != _mesEntity.CurrentStateName)
                    {
                        speedData.Value.IsActive = false;
                        _mesEntity.Animator.speed = speedData.Value.DefaultSpeed;
                        _isDirty = false;
                    }
                }
            }
        }
    }
}