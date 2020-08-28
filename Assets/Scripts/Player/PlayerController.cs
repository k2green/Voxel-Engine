using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

	public Transform cameraTransform;
	public float sensitivity = 100;
	public float speed = 1;

	private CharacterController charController;

	private void Start() {
		charController = GetComponent<CharacterController>();
	}

	private void Update() {
		Vector2 cameraMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;

		transform.Rotate(Vector3.up * cameraMovement.x);
		cameraTransform.Rotate(Vector3.left * cameraMovement.y);
	}

	private void FixedUpdate() {
		Vector3 playerMovement = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")) * speed * Time.fixedDeltaTime;

		charController.SimpleMove(transform.TransformDirection(playerMovement));
	}
}
