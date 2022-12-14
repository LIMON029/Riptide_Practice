using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform camProxy;
    [SerializeField] private float gravity;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;

    private float gravityAcceleration;
    private float moveSpeed;
    private float jumpSpeed;

    private bool[] inputs;
    private float yVelocity;

    private void OnValidate()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();
        inputs = new bool[6];
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])
            inputDirection.y += 1;
        if (inputs[1])
            inputDirection.y -= 1;
        if (inputs[2])
            inputDirection.x -= 1;
        if (inputs[3])
            inputDirection.x += 1;

        Move(inputDirection, inputs[4], inputs[5]);
    }

    private void Initialize()
    {
        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravityAcceleration);
    }

    public void Teleport(Vector3 toPosition)
    {
        bool isEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = toPosition;
        controller.enabled = isEnabled;
    }

    private void Move(Vector2 inputDirection, bool jump, bool sprint)
    {
        Vector3 moveDirection = Vector3.Normalize(camProxy.right * inputDirection.x + Vector3.Normalize(FlattenVector(camProxy.forward)) * inputDirection.y);
        moveDirection *= moveSpeed;

        if (sprint)
            moveDirection *= 2f;
        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (jump)
                yVelocity = jumpSpeed;
        }
        yVelocity += gravityAcceleration;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        SendMovement();
    }

    private Vector3 FlattenVector(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public void SetInput(bool[] _inputs, Vector3 _forward)
    {
        inputs = _inputs;
        camProxy.forward = _forward;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
