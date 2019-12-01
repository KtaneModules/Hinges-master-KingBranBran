using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class Hinges : MonoBehaviour {

	public KMSelectable module;
	public GameObject ModelComponent;
	public GameObject[] hingeGameObject;
	public KMSelectable[] hinge;
	public Material unicorn;
	private int[] hingeStatus = new int[8];

    void Awake()
    {
        // The wierd stuff at end of condition is for unicorn.
        for (int i = 0; (hingeStatus.Sum() < 2 || hingeStatus.Sum() > 6) && (i < 1 || 0 != Random.Range(0, 10)); i++) {
			hingeStatus = new[] {Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2)};
		}

		if (hingeStatus.Sum() < 2 || hingeStatus.Sum() > 7) { // If there is rare unicorn, only 1 hinge.
			ModelComponent.GetComponent<MeshRenderer>().material = unicorn;
			hingeStatus = new[] {0, 0, 0, 0, 0, 0, 0, 0};
			hingeStatus[Random.Range(0, 8)] = 1;
		}
	}
	void Start () {
		
	}

	void Update () {

		for (int i = 0; i < hingeGameObject.Length; i++) {
            var hingeEnabled = hingeStatus[i] == 0 ? false : true;
            hingeGameObject[i].GetComponent<MeshRenderer>().enabled = hingeEnabled;
            hingeGameObject[i].GetComponent<BoxCollider>().enabled = hingeEnabled;
		}
	}
}
