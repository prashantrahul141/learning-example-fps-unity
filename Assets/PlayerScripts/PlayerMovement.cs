using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody PlayerRB;
    const float playerSpeed = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        PlayerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        float horizontalInput = (Input.GetAxis("Horizontal") * playerSpeed);
        float verticalInput = (Input.GetAxis("Vertical") * playerSpeed);

        PlayerRB.position += new Vector3(horizontalInput, 0, verticalInput);


    }
}
