using UnityEngine;

public class KeedAlive : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		DontDestroyOnLoad(this.gameObject);
	}
	
}
