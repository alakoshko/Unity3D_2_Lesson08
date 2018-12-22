using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.CrossPlatformInput;

public class CharacterPanel : MonoBehaviour {

	public GameObject character;
	public Transform weaponsPanel;
	public Button buttonPrefab;
	//public Slider motionSpeed;
    [Range(1f, 10f)] [SerializeField] float m_MotionSpeed = 2f;

    Actions actions;
	PlayerController controller;
	Camera[] cameras;
    private int _currentCamera;

    /// <summary>
    /// added TDA
    /// </summary>
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
    

    void Start () {
		Initialize ();
	}

	void Initialize () {
		actions = character.GetComponent<Actions> ();
		controller = character.GetComponent<PlayerController> ();

        //CreateActionButton("Stay");
        //CreateActionButton("Walk");
        //CreateActionButton("Run");
        //CreateActionButton("Sitting");
        //CreateActionButton("Jump");
        //CreateActionButton("Aiming");
        //CreateActionButton("Attack");
        //CreateActionButton("Damage");
        //CreateActionButton("Death Reset", "Death");

        cameras = GameObject.FindObjectsOfType<Camera> ();
		var sort = from s in cameras orderby s.name select s;
        _currentCamera = 0;
        ShowCamera(sort.First<Camera>());
        m_Cam = cameras[_currentCamera % cameras.Length].transform;

        //motionSpeed.value = m_MotionSpeed;
    }

	void CreateWeaponButton(string name) {
		Button button = CreateButton (name, weaponsPanel);
		button.onClick.AddListener(() => controller.SetArsenal(name));
	}
    void ChangeWeaponButton(string wpname)
    {
        if(weaponsPanel.childCount > 0)
            Destroy(weaponsPanel.GetChild(0).gameObject);
     
        CreateWeaponButton(wpname);
    }


    Button CreateButton(string name, Transform group) {
		GameObject obj = (GameObject) Instantiate (buttonPrefab.gameObject);
		obj.name = name;
		obj.transform.SetParent(group);
		obj.transform.localScale = Vector3.one;
		Text text = obj.transform.GetChild (0).GetComponent<Text> ();
		text.text = name;
		return obj.GetComponent<Button> ();
	}

    public Camera ChangeCamers()
    {
        _currentCamera++;
        var cam = cameras[_currentCamera % cameras.Length];
        ShowCamera(cam);

        return cam;
    }
    void ShowCamera (Camera cam) {
		foreach (Camera c in cameras)
			c.gameObject.SetActive(c == cam);
	}

    private void Update() {
		Time.timeScale = m_MotionSpeed;

        if (Input.GetButtonDown("ChangeWeapons"))
            ChangeWeaponButton(controller.ChangeArsenal().name);

        if (Input.GetButtonDown("ChangeCamers"))
            ChangeCamers();

    }



    //private void FixedUpdate()
    //{
//        // read inputs
//        float h = CrossPlatformInputManager.GetAxis("Horizontal");
//        float v = CrossPlatformInputManager.GetAxis("Vertical");
//        bool crouch = Input.GetKey(KeyCode.C);

      

//        // calculate move direction to pass to character
//        if (m_Cam != null)
//        {
//            // calculate camera relative direction to move:
//            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
//            m_Move = v * m_CamForward + h * m_Cam.right;
//        }
//        else
//        {
//            // we use world-relative directions in the case of no main camera
//            m_Move = v * Vector3.forward + h * Vector3.right;
//        }
//#if !MOBILE_INPUT
//        // walk speed multiplier
//        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
//#endif

//        // pass all parameters to the character control script
//        controller.Move(m_Move, crouch, m_Jump);
//        m_Jump = false;
    //}

}
