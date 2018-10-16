using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbUIController : MonoBehaviour {
    [System.Serializable]
    public class CanvasSet
    {
        public List<Canvas> Canvases;
    }

    public CanvasSet[] Sets;

    public float RotationSpeed = 3f;


    private HashSet<Canvas> _allCanvases;
    private int _counter = 0;
    private float _lastRotationAt = 0f;

	// Use this for initialization
	void Start () {
        _allCanvases = new HashSet<Canvas>();
        foreach ( var set in Sets)
        {
            foreach( var canvas in set.Canvases)
            {
                _allCanvases.Add(canvas);
            }
        }
	}
	
	void Update()
    {
        if( Time.time - _lastRotationAt > RotationSpeed )
        {
            _lastRotationAt = Time.time;

            var set = Sets[_counter];
            foreach (var canvas in _allCanvases)
            {
                canvas.enabled = set.Canvases.Contains(canvas);
            }

            _counter = (_counter + 1) % Sets.Length;
        }
    }
}
