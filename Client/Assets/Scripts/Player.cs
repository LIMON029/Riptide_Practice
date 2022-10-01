using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    [SerializeField] private Transform camTransform;

    private string username;

    private void OnDestroy()
    {
        players.Remove(Id);
    }

    public static void Spawn(ushort _id, string _username, Vector3 _position)
    {
        Player _player;
        if(_id == NetworkManager.Singleton.Client.Id)
        {
            _player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, _position, Quaternion.identity).GetComponent<Player>();
            _player.IsLocal = true;
        }
        else
        {
            _player = Instantiate(GameLogic.Singleton.PlayerPrefab, _position, Quaternion.identity).GetComponent<Player>();
            _player.IsLocal = false;
        }
        _player.name = $"Player {_id} ({(string.IsNullOrEmpty(_username) ? "Guest" : _username)})";
        _player.Id = _id;
        _player.username = string.IsNullOrEmpty(_username) ? $"Guest {_id}" : _username;

        players.Add(_id, _player);
    }

    public void Move(Vector3 newPosition, Vector3 _forward)
    {
        transform.position = newPosition;
        if (IsLocal)
        {
            camTransform.forward = _forward;
        }
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (players.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetVector3(), message.GetVector3());
    }
    #endregion
}
