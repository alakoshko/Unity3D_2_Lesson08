using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterViewer : MonoBehaviour {
	
	//public Transform cameras;

	//Transform targetForCamera;
	//Vector3 deltaPosition;
	//Vector3 lastPosition = Vector3.zero;
	//bool rotating = false;

	//void Awake () {
	//	targetForCamera = GameObject.Find ("RigSpine3").transform;
	//	deltaPosition = cameras.position - targetForCamera.position;
	//}

	//void Update () {
 //       //if (Input.GetMouseButtonDown (0) && Input.mousePosition.x < Screen.width * 0.6f) {
        
 //       if (Input.mousePosition.x < Screen.width * 0.6f)
 //       {
 //           //Debug.Log(Input.mousePosition.x);
 //           lastPosition = Input.mousePosition;
	//		rotating = true;
	//	}

	//	if (Input.GetKey(KeyCode.Escape))
	//		rotating = false;
		
	//	//if (rotating)
	//	//	transform.Rotate(0, -300f * (Input.mousePosition - lastPosition).x / Screen.width, 0);

	//	lastPosition = Input.mousePosition;
	//}

 //   void LateUpdate()
 //   {
 //       cameras.position += (targetForCamera.position + deltaPosition - cameras.position) * Time.unscaledDeltaTime * 5;
 //   }
}
