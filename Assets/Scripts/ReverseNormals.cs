using UnityEngine;
using System.Collections;
[RequireComponent(typeof(MeshFilter))]
public class ReverseNormals : MonoBehaviour {

	public bool isReversed = false;

	// Use this for initialization
	void Start () {
		if (!isReversed)
			Reverse (); //Starting up with a fixation paradigm: therefore do not reserve at start
    }
	public void Reverse()
	{
		MeshFilter filter = GetComponent<MeshFilter>();
		Mesh mesh = filter.mesh;
		//Debug.Log(mesh);		
		Vector3[] normals = mesh.normals;
		//Debug.Log(normals.Length);
		for (int i = 0; i < normals.Length; i++)
			normals[i] = -normals[i];
		mesh.normals = normals;
		
		for (int m = 0; m < mesh.subMeshCount; m++)
		{
			int[] triangles = mesh.GetTriangles(m);
			//Debug.Log (triangles.Length);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int temp = triangles[i + 0];
				triangles[i + 0] = triangles[i + 1];
				triangles[i + 1] = temp;
			}
			mesh.SetTriangles(triangles, m);
		}
		isReversed = !isReversed;
	}

}
