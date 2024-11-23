using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LagCompensation
{
    public static List<WorldState> worldStates = new List<WorldState>();

    public static void Start(int maxPlayers)
    {
        worldStates = new List<WorldState>();
    }

    public static void Stop()
    {
        worldStates.Clear();
    }

    public static void Backtrack(int client, int tick)
    {
        if (!Server.clients[client].player)
            return;

        List<Vector3> debugPositions = new List<Vector3>();
        Dictionary<int, ClientState> backup = new Dictionary<int, ClientState>();

        foreach (var otherClient in Server.clients.Where(c => c.Key != client && c.Value.player))
        {
            int id = otherClient.Key;
            Player player = otherClient.Value.player;
            backup[id] = Backup(player);
            Vector3 position = BacktrackPlayer(player, tick, client, id);
            debugPositions.Add(position);
        }

        if (Server.clients[client].player.lagCompDebug)
        {
            ServerSend.LagCompDebug(debugPositions, client);
        }
    }

    private static ClientState Backup(Player player)
    {
        return new ClientState
        {
            id = player.id,
            position = player.GetPosition(),
            rotation = player.GetOrientation(),
        };
    }

    public static Vector3 BacktrackPlayer(Player player, int tick, int client, int targetClient)
    {
        WorldState state = worldStates.OrderBy(ws => Mathf.Abs(ws.tick - tick)).FirstOrDefault(ws => ws.states.Any(s => s.id == player.id));

        if (state == null)
        {
            return Vector3.zero;
        }

        ClientState clientState = state.states.First(s => s.id == player.id);
        NetworkManager.Instance.RollbackPlayer(clientState.position, clientState.rotation, client, targetClient);
        return clientState.position;
    }

    private static void AddPlayerState(Player player)
    {
        WorldState state = worldStates.FirstOrDefault(ws => ws.tick == player.tick);

        if (state == null)
        {
            state = new WorldState { tick = player.tick };
            worldStates.Add(state);
        }

        state.states.Add(new ClientState
        {
            id = player.id,
            position = player.GetPosition(),
            rotation = player.GetOrientation(),
        });
    }

    public static void SavePlayerStates()
    {
        foreach (var client in Server.clients)
        {
            if (client.Value.player != null)
            {
                AddPlayerState(client.Value.player);
            }
        }

        worldStates.RemoveAll(ws => NetworkManager.tick - ws.tick > 50);
    }

    public static void DeletePlayerStates(int playerId)
    {
        foreach (var state in worldStates)
        {
            state.states.RemoveAll(s => s.id == playerId);
        }

        worldStates.RemoveAll(ws => ws.states.Count == 0);
    }
}
