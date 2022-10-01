using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<int, Player> players = new Dictionary<int, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;

    private void OnDestroy()
    {
        players.Remove(Id);
    }

    public static void Spawn(ushort _id, string _username)
    {
        foreach (Player otherPlayer in players.Values)
            otherPlayer.SendSpawned(_id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {_id} ({(string.IsNullOrEmpty(_username) ? "Guest" : _username)})";
        player.Id = _id;
        player.Username = string.IsNullOrEmpty(_username) ? $"Guest {_id}" : _username;

        player.SendSpawned();
        players.Add(_id, player);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort _fromClientId, Message _message)
    {
        Spawn(_fromClientId, _message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort _fromClientId, Message _message)
    {
        if (players.TryGetValue(_fromClientId, out Player player))
            player.Movement.SetInput(_message.GetBools(6), _message.GetVector3());
    }
    #endregion
}
