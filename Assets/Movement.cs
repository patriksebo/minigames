using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Movement : NetworkBehaviour
{
    public CharacterController controller;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    public Transform cam;

    public GameObject camera;
    public GameObject cameraAdjuster;

    public Animator anim;
    public bool walking = false;

    // TEST
    public GameObject[] players;
    public float gravity = 1;
    Vector3 velocity;
    public bool isGrounded;
    public float jumpHeight = 2;

    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;

    public bool isDisabled = false;
    public bool isDisabledMoving = false;
    public bool isRecentJump = false;

    public GameObject res;
    public GameObject nametag;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (!isLocalPlayer)
        {
            camera.SetActive(false);
            cameraAdjuster.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if(isDisabled == false)
        {
            //jump
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -10f;
                isDisabledMoving = false;
            }
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && isDisabledMoving == false && isRecentJump == false)
            {
                //anim.Play("Jumping");
                CmdAnimJump();
                velocity.y = 3; //Mathf.Sqrt(jumpHeight * -2 * gravity);
                isDisabledMoving = true;
                isRecentJump = true;
                Invoke("JumpFade", 3);
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);


            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            //walking = false;
            anim.SetBool("isWalking", false);

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                if(isDisabledMoving == false)
                {
                    anim.SetBool("isWalking", true);
                    Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    controller.Move(moveDir.normalized * speed * Time.deltaTime);
                }
                else
                {
                    //anim.SetBool("isWalking", true);
                    Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    controller.Move(moveDir.normalized * speed * 0.3f * Time.deltaTime);
                }

            }
        }




        //dance
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //anim.Play("Standing Melee Kick");

            players = GameObject.FindGameObjectsWithTag("Player");
            CmdTeleport();
        }
        */


        if(Input.GetKeyDown(KeyCode.E))
        {
            CmdSetNameTagColor(Color.red);
        }
    }



    // ------------ TEST
    public void E(Vector3 vector)
    {
        isDisabled = true;
        Debug.Log("E");
        CmdTeleport(vector);
        Invoke("E_end", 1);
    }
    public void E_end()
    {
        isDisabled = false;
    }


    [Command]
    public void CmdAnimJump()
    {
        RpcAnimJump();
    }
    [ClientRpc]
    public void RpcAnimJump()
    {
        anim.Play("Jumping");
    }
    public void JumpFade()
    {
        isRecentJump = false;
    }


    public void Teleport(Vector3 vector) 
    {
        isDisabled = true;
        this.transform.position = vector;
        Invoke("TeleportFade", 1);
    }
    public void TeleportFade()
    {
        isDisabled = false;
    }


    [Command]
    public void CmdTeleport(Vector3 vector)
    {
        Debug.Log("CmdTeleport");
        RpcTeleport(vector);
    }

    [ClientRpc]
    private void RpcTeleport(Vector3 vector)
    {
        Debug.Log("RpcTeleport");
        //this.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        //anim.Play("Standing Melee Kick");

        //GameObject.FindGameObjectsWithTag("Player")[0].transform.name = "RpcTeleport";
        //GameObject.FindGameObjectsWithTag("Player")[1].transform.name = "RpcTeleport";


        //GameObject.FindGameObjectsWithTag("Player")[0].transform.position = new Vector3(0.0f, 2.0f, 0.0f);
        //GameObject.FindGameObjectsWithTag("Player")[1].transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            //p.transform.position = new Vector3(0.0f, 2.0f, 0.0f);
            p.transform.position = vector;
        }
    }


    public void WinLose(bool isWin)
    {
        if (!isLocalPlayer)
            return;

        res.SetActive(true);

        if(isWin)
        {
            res.GetComponent<Text>().text = "Vyhral si!";
            res.GetComponent<Text>().color = Color.green;
        }
        else
        {
            res.GetComponent<Text>().text = "Prehral si!";
            res.GetComponent<Text>().color = Color.red;
        }

        Invoke("WinLoseFade", 5);
    }
    public void WinLoseFade()
    {
        res.SetActive(false);
    }



    [Command]
    public void CmdSetNameTagColor(Color color)
    {
        RpcSetNameTagColor(color);
    }
    [ClientRpc]
    public void RpcSetNameTagColor(Color color)
    {
        nametag.GetComponent<Image>().color = color;
    }


    public void Goal(bool isGood)
    {
        if (!isLocalPlayer)
            return;

        res.SetActive(true);
        res.GetComponent<Text>().text = "Goal!";

        if (isGood)
        {
            res.GetComponent<Text>().color = Color.green;
        }
        else
        {
            res.GetComponent<Text>().color = Color.red;
        }

        Invoke("GoalFade", 5);
    }
    public void GoalFade()
    {
        res.SetActive(false);
    }
}