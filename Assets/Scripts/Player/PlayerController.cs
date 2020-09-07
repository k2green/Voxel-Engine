using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

	public Transform cameraTransform;
	public float sensitivity = 100;
	public float speed = 1;

	private CharacterController charController;
	private bool isPaused;

	private void Start() {
		isPaused = false;
		charController = GetComponent<CharacterController>();

		Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			isPaused = !isPaused;

			Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = isPaused;
		}

		if (!isPaused) {
			Vector2 cameraMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;

			transform.Rotate(Vector3.up * cameraMovement.x);
			cameraTransform.Rotate(Vector3.left * cameraMovement.y);
		}
	}

	private void FixedUpdate() {
		if (!isPaused) {
			Vector3 playerMovement = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")) * speed * Time.fixedDeltaTime;

			charController.SimpleMove(transform.TransformDirection(playerMovement));
		}
	}
}
