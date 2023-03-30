using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour {

	public float CameraMoveSpeed = 120.0f;
	public GameObject CameraFollowObj;
	Vector3 FollowPOS;
	public float clampAngle = 80.0f;
	public float inputSensitivity = 150.0f;
	public GameObject CameraObj;
	public GameObject PlayerObj;
	public float camDistanceXToPlayer;
	public float camDistanceYToPlayer;
	public float camDistanceZToPlayer;
	public float mouseX;
	public float mouseY;
	public float finalInputX;
	public float finalInputZ;
	public float smoothX;
	public float smoothY;
	private float rotY = 0.0f;
	private float rotX = 0.0f;

	private float inputX = 0.0f;
	private float inputZ = 0.0f;
	private bool mouseLocked;

	[SerializeField] private GameObject pauseMenu;

	// Use this for initialization
	void Start () {
		if (!IsOwner) Destroy(gameObject);
		Vector3 rot = transform.localRotation.eulerAngles;
		rotY = rot.y;
		rotX = rot.x;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		mouseLocked = true;
		pauseMenu = GameObject.FindGameObjectWithTag("Pause");
		pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F1) && mouseLocked == true){
			mouseLocked = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else if (Input.GetKeyDown(KeyCode.F1) && mouseLocked == false){
			mouseLocked = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		if (Input.GetKeyDown(KeyCode.Escape) && mouseLocked == true){
			mouseLocked = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			pauseMenu.SetActive(true);
		} else if (Input.GetKeyDown(KeyCode.Escape) && mouseLocked == false){
			mouseLocked = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			
			pauseMenu.SetActive(false);
		}

		inputX = 0;
		inputZ = 0;
		mouseX = 0;
		mouseY = 0;

		// We setup the rotation of the sticks here
		if (mouseLocked == true) {
			inputX = Input.GetAxis ("RightStickHorizontal");
			inputZ = Input.GetAxis ("RightStickVertical");
			mouseX = Input.GetAxis ("Mouse X");
			mouseY = Input.GetAxis ("Mouse Y");
		}
		finalInputX = inputX + mouseX;
		finalInputZ = inputZ + mouseY;

		rotY += finalInputX * inputSensitivity * Time.deltaTime;
		rotX += finalInputZ * inputSensitivity * Time.deltaTime;

		rotX = Mathf.Clamp (rotX, -clampAngle, clampAngle);

		Quaternion localRotation = Quaternion.Euler (rotX, rotY, 0.0f);
		transform.rotation = localRotation;
	}

	void LateUpdate () {
		CameraUpdater ();
	}

	void CameraUpdater() {
		// set the target object to follow
		Transform target = CameraFollowObj.transform;

		//move towards the game object that is the target
		float step = CameraMoveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, target.position, step);
	}
}
