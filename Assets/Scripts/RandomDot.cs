using UnityEngine;
using System;

public class RandomDot : MonoBehaviour {

	private RotatingRandomDots dotParams;
	private int remainingLifeTime;
    private int lifeTime = 1;



    // Use this for initialization
    void Start () {
		dotParams = GameObject.Find ("DotMotionEntries").GetComponent<RotatingRandomDots> ();
        transform.position = dotParams.distance * UnityEngine.Random.onUnitSphere;
        transform.LookAt(dotParams.rotCenterTransform.position);
        ChangeLifeCycle(dotParams.numOfCycles);
    }

    // Update is called once per frame
    void Update () {
        if (lifeTime != dotParams.numOfCycles)
            ChangeLifeCycle(dotParams.numOfCycles);
		remainingLifeTime--;
        if (remainingLifeTime == 0)
        {
            remainingLifeTime = lifeTime;
            transform.position = dotParams.distance * UnityEngine.Random.onUnitSphere;
            transform.LookAt(dotParams.rotCenterTransform.position);
        } else {
            transform.RotateAround(dotParams.rotCenterTransform.position, dotParams.rotAxis, dotParams.rotAngle);
        }

	}
    private void ChangeLifeCycle(int numOfCycle)
    {
        lifeTime = dotParams.numOfCycles;
        remainingLifeTime = (int)UnityEngine.Random.Range(1f, (float)lifeTime);
    }
}
