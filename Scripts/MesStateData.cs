using UnityEngine;

namespace MecanimEventSystem
{
    [System.Serializable]
    public class MesStateData
    {
        [SerializeField] private string _stateName;
        [SerializeField] private string _layerName;

        [SerializeField] private int _tag;
        [SerializeField] private int _nameHash;

        [SerializeField] private bool _isDefaultState;
        [SerializeField] private string _id;

        public string StateName
        {
            get { return _stateName; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't change state name, while playing");
                    return;
                }

                _stateName = value;
                _nameHash = Animator.StringToHash(_layerName + "." + _stateName);
            }
        }

        public int Tag
        {
            get { return _tag; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't change state tag, while playing");
                    return;
                }

                _tag = value;
            }
        }

        public int NameHash
        {
            get { return _nameHash; }
        }

        public string LayerName
        {
            get { return _layerName; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't change state layer, while playing");
                    return;
                }
                _layerName = value;
                _nameHash = Animator.StringToHash(_layerName + "." + _stateName);
                
            }
        }

        public bool IsDefaultState
        {
            get { return _isDefaultState; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't change state default flag, while playing");
                    return;
                }

                _isDefaultState = value;
            }
        }

        public string Id
        {
            get { return _id; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't change state Id, while playing");
                    return;
                }
                _id = value;
            }
        }

        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(_stateName) || string.IsNullOrEmpty(_layerName));
        }
    }
}